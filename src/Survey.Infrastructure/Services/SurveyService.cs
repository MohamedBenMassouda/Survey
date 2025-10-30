using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Enums;
using Survey.Infrastructure.Extensions;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Services;

public class SurveyService(IUnitOfWork unitOfWork) : ISurveyService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<SurveyDto>> GetSurveysAsync(SurveyQueryParams query)
    {
        // Build filter expression using PredicateBuilder
        var filter = PredicateBuilder.True<SurveyModel>();

        // Filter by status
        if (query.Status.HasValue)
        {
            filter = filter.And(s => s.Status == query.Status.Value);
        }

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
            s => s.Questions.OrderBy(q => q.DisplayOrder)
        );

        if (survey == null)
            return null;

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
            Questions = survey.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.Type,
                IsRequired = q.IsRequired,
                DisplayOrder = q.DisplayOrder
            }).ToList(),
            CreatedAt = survey.CreatedAt
        };
    }

    // ============================================
    // CREATE SURVEY WITH QUESTIONS (TRANSACTION)
    // ============================================

    public async Task<SurveyDto> CreateSurveyAsync(CreateSurveyRequest request, Guid adminId)
    {
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
            await _unitOfWork.SaveChangesAsync();

            // Create questions
            if (request.Questions.Any())
            {
                var questions = request.Questions.Select((q, index) => new Question
                {
                    SurveyId = survey.Id,
                    QuestionText = q.QuestionText,
                    Type = Enum.Parse<QuestionType>(q.QuestionType),
                    IsRequired = q.IsRequired,
                    DisplayOrder = q.DisplayOrder ?? index + 1
                }).ToList();

                await _unitOfWork.Questions.AddRangeAsync(questions);
                await _unitOfWork.SaveChangesAsync();

                // Create options for choice-based questions
                foreach (var questionRequest in request.Questions)
                {
                    if (questionRequest.Options.Any())
                    {
                        var questionType = Enum.Parse<QuestionType>(questionRequest.QuestionType);

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

                await _unitOfWork.SaveChangesAsync();
            }

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
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
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
            return null;
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
            return false;
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
            return null;
        }

        if (survey.Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("Only draft surveys can be published");
        }

        if (!survey.Questions.Any())
        {
            throw new InvalidOperationException("Survey must have at least one question");
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
            throw new ArgumentException("Survey not found");

        if (survey.Status != SurveyStatus.Published)
            throw new InvalidOperationException("Survey must be published to generate tokens");

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
            return null;
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
}