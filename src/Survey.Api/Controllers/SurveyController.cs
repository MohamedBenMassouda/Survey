using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

public class SurveyController(IUnitOfWork unitOfWork, ISurveyService surveyService)
    : BaseController<SurveyModel>(unitOfWork)
{
    private readonly ISurveyService _surveyService = surveyService;

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

            // var adminId = GetCurrentAdminId();
            var survey = await _surveyService.CreateSurveyAsync(request, Guid.Empty);

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
}