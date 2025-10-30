using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Core;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

[ApiController]
[Route("api/[controller]s")]
public class SurveyController(
    IUnitOfWork unitOfWork,
    ISurveyService surveyService,
    ICurrentUser currentUser,
    IConfiguration configuration)
    : ControllerBase
{
    private readonly ISurveyService _surveyService = surveyService;
    private readonly ICurrentUser _currentUser = currentUser;
    private string _frontendBaseUrl = configuration["Frontend:Url"]!;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SurveyDto>), 200)]
    public new async Task<IActionResult> GetAll([FromQuery] SurveyQueryParams query)
    {
        var surveys = await _surveyService.GetSurveysAsync(query);

        return Ok(surveys);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SurveyDetailDto), 200)]
    [ProducesResponseType(404)]
    public new async Task<IActionResult> GetById(Guid id)
    {
        var survey = await _surveyService.GetSurveyByIdAsync(id);
        return Ok(survey);
    }

    [Authorize]
    [HttpGet("{id}/analytics")]
    [ProducesResponseType(typeof(SurveyAnalyticsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSurveyAnalytics(Guid id)
    {
        var analytics = await _surveyService.GetAnalyticsAsync(id);
        return Ok(analytics);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedSurveys([FromQuery] PaginationParams paginationParams)
    {
        var surveys = await _surveyService.GetPublishedSurveysAsync(paginationParams);

        return Ok(surveys);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SurveyDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyRequest request)
    {
        var adminId = _currentUser.GetCurrentUserId();
        var survey = await _surveyService.CreateSurveyAsync(request, adminId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = survey.Id },
            survey
        );
    }

    [Authorize]
    [HttpPost("invitations")]
    [ProducesResponseType(typeof(InvitationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendInvitations([FromBody] SendInvitationRequest request)
    {
        var result = await _surveyService.SendInvitationsAsync(request, _frontendBaseUrl);

        return Ok(result);
    }

    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(SurveyDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishSurvey(Guid id)
    {
        var survey = await _surveyService.PublishSurveyAsync(id);
        
        return Ok(survey);
    }
}