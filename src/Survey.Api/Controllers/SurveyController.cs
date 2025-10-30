using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Core;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

[ApiController]
[Route("api/[controller]s")]
public class SurveyController(IUnitOfWork unitOfWork, ISurveyService surveyService, ICurrentUser currentUser)
    : ControllerBase
{
    private readonly ISurveyService _surveyService = surveyService;
    private readonly ICurrentUser _currentUser = currentUser;

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SurveyDto>), 200)]
    public new async Task<IActionResult> GetAll([FromQuery] SurveyQueryParams query)
    {
        var surveys = await _surveyService.GetSurveysAsync(query);
        
        return Ok(surveys);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SurveyDetailDto), 200)]
    [ProducesResponseType(404)]
    public new async Task<IActionResult> GetById(Guid id)
    {
        var survey = await _surveyService.GetSurveyByIdAsync(id);

        if (survey == null)
        {
            return NotFound(new { error = $"Survey with ID {id} not found" });
        }

        return Ok(survey);
    }

    [Authorize]
    [HttpGet("{id}/analytics")]
    [ProducesResponseType(typeof(SurveyAnalyticsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSurveyAnalytics(Guid id)
    {
        try
        {
            var analytics = await _surveyService.GetAnalyticsAsync(id);

            if (analytics == null)
                return NotFound(new { error = $"Survey with ID {id} not found" });

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { error = "An error occurred while retrieving analytics", details = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedSurveys([FromQuery] PaginationParams paginationParams)
    {
        var surveys = await _surveyService.GetPublishedSurveysAsync(paginationParams);

        return Ok(surveys);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(SurveyDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyRequest request)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Survey title is required" });
            }

            if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate < request.StartDate)
            {
                return BadRequest(new { error = "End date must be after start date" });
            }

            var adminId = _currentUser.GetCurrentUserId();
            var survey = await _surveyService.CreateSurveyAsync(request, adminId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = survey.Id },
                survey
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while creating the survey", details = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("invitations")]
    [ProducesResponseType(typeof(InvitationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendInvitations([FromBody] SendInvitationRequest request)
    {
        try
        {
            // Validation
            if (request.RecipientEmails == null || !request.RecipientEmails.Any())
            {
                return BadRequest(new { error = "At least one recipient email is required" });
            }

            // Get base URL from request
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = await _surveyService.SendInvitationsAsync(request, baseUrl);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while sending invitations", details = ex.Message });
        }
    }
}