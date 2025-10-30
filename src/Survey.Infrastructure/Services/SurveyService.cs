using Survey.Core;
using Survey.Core.Exceptions;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Enums;
using Survey.Infrastructure.Extensions;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Services;

public class SurveyService(IUnitOfWork unitOfWork, IEmailService emailService, ICurrentUser currentUser) : ISurveyService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<PagedResult<SurveyDto>> GetSurveysAsync(SurveyQueryParams query)
    {
        // Build filter expression using PredicateBuilder
        var filter = PredicateBuilder.True<SurveyModel>();

        // Filter by status
        if (query.Status.HasValue)
        {
            filter = filter.And(s => s.Status == query.Status.Value);
        }

        /*if (!_currentUser.IsLoggedIn())
        {
            filter = filter.And<SurveyModel>(s => s.Status == SurveyStatus.Published &&
                s.StartDate <= DateTime.UtcNow &&
                s.EndDate >= DateTime.UtcNow);
        }*/
        
        // Filter by creator
        if (query.CreatedByAdminId.HasValue)
        {
            filter = filter.And(s => s.CreatorId == query.CreatedByAdminId.Value);
        }

        // Filter by date range
        if (query.StartDateFrom.HasValue)
        {
            filter = filter.And(s => s.StartDate >= query.StartDateFrom.Value);
        }

        if (query.StartDateTo.HasValue)
        {
            filter = filter.And(s => s.StartDate <= query.StartDateTo.Value);
        }

        // Build sorting
        Func<IQueryable<SurveyModel>, IOrderedQueryable<SurveyModel>>? orderBy = null;

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = surveyQuery => query.SortBy.ToLower() switch
            {
                "title" => query.IsAscending
                    ? surveyQuery.OrderBy(s => s.Title)
                    : surveyQuery.OrderByDescending(s => s.Title),
                "createdat" => query.IsAscending
                    ? surveyQuery.OrderBy(s => s.CreatedAt)
                    : surveyQuery.OrderByDescending(s => s.CreatedAt),
                "status" => query.IsAscending
                    ? surveyQuery.OrderBy(s => s.Status)
                    : surveyQuery.OrderByDescending(s => s.Status),
                _ => surveyQuery.OrderByDescending(s => s.CreatedAt)
            };
        }
        else
        {
            orderBy = q => q.OrderByDescending(s => s.CreatedAt);
        }

        // Get paginated results with DTO projection
        var pagedResult = await _unitOfWork.Surveys.GetPagedAsync<SurveyDto>(
            query.PageNumber,
            query.PageSize,
            filter: filter,
            orderBy: orderBy,
            selector: s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Status = s.Status.ToString(),
                CreatedByName = s.Creator.FullName,
                QuestionCount = s.Questions.Count,
                ResponseCount = s.Responses.Count(r => r.IsCompleted),
                CreatedAt = s.CreatedAt
            }
        );

        return pagedResult;
    }

    public async Task<PagedResult<SurveyDto>> GetPublishedSurveysAsync(PaginationParams paginationParams)
    {
        var filter = PredicateBuilder.True<SurveyModel>()
            .And(s => s.Status == SurveyStatus.Published)
            .And(s => s.StartDate <= DateTime.UtcNow)
            .And(s => s.EndDate >= DateTime.UtcNow);

        var pagedResult = await _unitOfWork.Surveys.GetPagedAsync<SurveyDto>(
            pageNumber: paginationParams.PageNumber,
            pageSize: paginationParams.PageSize,
            filter: filter,
            orderBy: q => q.OrderByDescending(s => s.CreatedAt),
            selector: s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Status = s.Status.ToString(),
                CreatedByName = s.Creator.FullName,
                QuestionCount = s.Questions.Count,
                ResponseCount = s.Responses.Count(r => r.IsCompleted),
                CreatedAt = s.CreatedAt
            }
        );

        return pagedResult;
    }

    public async Task<SurveyDetailDto> GetSurveyByIdAsync(Guid id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(
            id,
            s => s.Creator,
            s => s.Questions
        );

        if (survey == null)
            throw new NotFoundException($"Survey with ID {id} not found.");

        // Load question options for all questions
        var questionIds = survey.Questions.Select(q => q.Id).ToList();
        var allOptions = await _unitOfWork.QuestionOptions.FindAsync(o => questionIds.Contains(o.QuestionId));
        
        // Group options by question
        var optionsByQuestion = allOptions.GroupBy(o => o.QuestionId).ToDictionary(g => g.Key, g => g.ToList());

        return new SurveyDetailDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            Status = survey.Status.ToString(),
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            CreatedBy = new AdminDto
            {
                Id = survey.Creator.Id,
                Email = survey.Creator.Email,
                FullName = survey.Creator.FullName
            },
            Questions = survey.Questions.OrderBy(q => q.DisplayOrder).Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.Type,
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder,
                Options = optionsByQuestion.ContainsKey(q.Id)
                    ? optionsByQuestion[q.Id].OrderBy(o => o.DisplayOrder).Select(o => new QuestionOptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        DisplayOrder = o.DisplayOrder
                    }).ToList()
                    : new List<QuestionOptionDto>()
            }).ToList(),
            CreatedAt = survey.CreatedAt
        };
    }

    public async Task<SurveyDto> CreateSurveyAsync(CreateSurveyRequest request, Guid adminId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BadRequestException("Survey title is required.");
        }

        if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate < request.StartDate)
        {
            throw new BadRequestException("End date must be after start date.");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Create survey
            var survey = new SurveyModel
            {
                Title = request.Title,
                Description = request.Description,
                Status = SurveyStatus.Draft,
                CreatorId = adminId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _unitOfWork.Surveys.AddAsync(survey);

            // Create questions
            if (request.Questions.Any())
            {
                var questions = request.Questions.Select((q, index) => new Question
                {
                    SurveyId = survey.Id,
                    QuestionText = q.QuestionText,
                    Type = q.QuestionType,
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder ?? index + 1
                }).ToList();

                await _unitOfWork.Questions.AddRangeAsync(questions);

                // Create options for choice-based questions
                foreach (var questionRequest in request.Questions)
                {
                    if (questionRequest.Options.Any())
                    {
                        var questionType = questionRequest.QuestionType;

                        if (questionType != QuestionType.MultipleChoice &&
                            questionType != QuestionType.Checkbox &&
                            questionType != QuestionType.Dropdown &&
                            questionType != QuestionType.Scale)
                        {
                            continue;
                        }

                        var question = questions.First(q =>
                            q.QuestionText == questionRequest.QuestionText);

                        var options = questionRequest.Options.Select((opt, index) =>
                            new QuestionOption
                            {
                                QuestionId = question.Id,
                                OptionText = opt.OptionText,
                                DisplayOrder = opt.DisplayOrder ?? index + 1
                            }).ToList();

                        await _unitOfWork.QuestionOptions.AddRangeAsync(options);
                    }
                }
            }

            // Save all changes at once
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Return DTO
            var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);

            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Status = survey.Status.ToString(),
                CreatedByName = admin.FullName,
                QuestionCount = request.Questions?.Count ?? 0,
                ResponseCount = 0,
                CreatedAt = survey.CreatedAt
            };
        }
        catch (Exception ex) when (ex is not SurveyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new SurveyException($"Failed to create survey: {ex.Message}", ex);
        }
    }

    public async Task<SurveyDto?> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(
            id,
            s => s.Creator,
            s => s.Questions
        );

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {id} not found.");
        }

        survey.Title = request.Title ?? survey.Title;
        survey.Description = request.Description ?? survey.Description;
        survey.StartDate = request.StartDate ?? survey.StartDate;
        survey.EndDate = request.EndDate ?? survey.EndDate;
        survey.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Surveys.Update(survey);
        await _unitOfWork.SaveChangesAsync();

        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Status = survey.Status.ToString(),
            CreatedByName = survey.Creator.FullName,
            QuestionCount = survey.Questions.Count,
            ResponseCount = survey.Responses?.Count(r => r.IsCompleted) ?? 0,
            CreatedAt = survey.CreatedAt
        };
    }

    public async Task<bool> DeleteSurveyAsync(Guid id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {id} not found.");
        }

        _unitOfWork.Surveys.Delete(survey);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SurveyDto?> PublishSurveyAsync(Guid id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(
            id,
            s => s.Creator,
            s => s.Questions
        );

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {id} not found.");
        }

        if (survey.Status != SurveyStatus.Draft)
        {
            throw new BadRequestException("Only draft surveys can be published.");
        }

        if (!survey.Questions.Any())
        {
            throw new BadRequestException("Survey must have at least one question to be published.");
        }

        survey.Status = SurveyStatus.Published;
        survey.StartDate = DateTime.UtcNow;
        survey.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Surveys.Update(survey);
        await _unitOfWork.SaveChangesAsync();

        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Status = survey.Status.ToString(),
            CreatedByName = survey.Creator.FullName,
            QuestionCount = survey.Questions.Count,
            ResponseCount = 0,
            CreatedAt = survey.CreatedAt
        };
    }

    public async Task<List<string>> GenerateTokensAsync(Guid surveyId, int count, int expiryDays)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);

        if (survey == null)
            throw new NotFoundException($"Survey with ID {surveyId} not found.");

        if (survey.Status != SurveyStatus.Published)
            throw new BadRequestException("Survey must be published to generate tokens.");

        var tokens = new List<string>();
        var expiresAt = DateTime.UtcNow.AddDays(expiryDays);

        for (int i = 0; i < count; i++)
        {
            var token = GenerateSecureToken();

            await _unitOfWork.SurveyTokens.AddAsync(new SurveyToken
            {
                SurveyId = surveyId,
                Token = token,
            });

            tokens.Add(token);
        }

        await _unitOfWork.SaveChangesAsync();

        return tokens;
    }

    public async Task<SurveyAnalyticsDto?> GetAnalyticsAsync(Guid surveyId)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(
            surveyId,
            s => s.Questions,
            s => s.Tokens,
            s => s.Responses
        );

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {surveyId} not found.");
        }

        var completedResponses = survey.Responses.Count(r => r.IsCompleted);
        var totalTokens = survey.Tokens.Count;

        return new SurveyAnalyticsDto
        {
            SurveyId = survey.Id,
            SurveyTitle = survey.Title,
            TotalQuestions = survey.Questions.Count,
            TokensGenerated = totalTokens,
            TotalResponses = completedResponses,
            ResponseRate = totalTokens > 0 ? (double)completedResponses / totalTokens * 100 : 0,
            AverageCompletionTime = survey.Responses
                .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                .Select(r => (r.CompletedAt.Value - r.StartedAt).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average()
        };
    }

    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public async Task<InvitationResponse> SendInvitationsAsync(SendInvitationRequest request, string baseUrl)
    {
        var response = new InvitationResponse();
        var emailTokenMap = new Dictionary<string, string>();

        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId);

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found.");
        }

        if (survey.Status != SurveyStatus.Published)
        {
            throw new BadRequestException("Survey must be published to send invitations.");
        }

        if (request.RecipientEmails == null || !request.RecipientEmails.Any())
        {
            throw new BadRequestException("At least one recipient email is required.");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Generate tokens for each email
            foreach (var email in request.RecipientEmails)
            {
                try
                {
                    // Validate email format
                    if (!IsValidEmail(email))
                    {
                        response.FailedInvitations.Add(new InvitationError
                        {
                            Email = email,
                            ErrorMessage = "Invalid email format"
                        });
                        continue;
                    }

                    // Generate unique token
                    var token = GenerateSecureToken();

                    // Save token to database
                    await _unitOfWork.SurveyTokens.AddAsync(new SurveyToken
                    {
                        SurveyId = request.SurveyId,
                        Token = token,
                    });

                    emailTokenMap[email] = token;
                }
                catch (Exception ex)
                {
                    response.FailedInvitations.Add(new InvitationError
                    {
                        Email = email,
                        ErrorMessage = $"Token generation failed: {ex.Message}"
                    });
                }
            }

            // Only save tokens if at least one was generated successfully
            if (emailTokenMap.Any())
            {
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new BadRequestException("Failed to generate tokens for any recipient.");
            }

            // Send emails
            var surveyLink = $"{baseUrl}/surveys/{survey.Id}";
            
            foreach (var emailToken in emailTokenMap)
            {
                try
                {
                    await _emailService.SendSurveyInvitationAsync(
                        emailToken.Key,
                        survey.Title,
                        surveyLink,
                        emailToken.Value
                    );

                    response.SuccessfulInvitations.Add(emailToken.Key);
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.Message;
                    
                    // Extract user-friendly error message from email service exceptions
                    if (ex.Message.Contains("Key not found"))
                    {
                        errorMessage = "Email service configuration error. Please contact administrator.";
                    }
                    else if (ex.Message.Contains("unauthorized"))
                    {
                        errorMessage = "Email service authentication failed. Please contact administrator.";
                    }
                    else if (ex.Message.Contains("Invalid sender"))
                    {
                        errorMessage = "Sender email not verified. Please contact administrator.";
                    }
                    
                    response.FailedInvitations.Add(new InvitationError
                    {
                        Email = emailToken.Key,
                        ErrorMessage = errorMessage
                    });
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            response.TotalInvitations = request.RecipientEmails.Count;
            return response;
        }
        catch (Exception ex) when (ex is not SurveyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new SurveyException($"Failed to send invitations: {ex.Message}", ex);
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}