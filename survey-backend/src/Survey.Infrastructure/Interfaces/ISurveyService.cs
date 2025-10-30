using Survey.Infrastructure.DTO;

namespace Survey.Infrastructure.Interfaces;

public interface ISurveyService
{
    Task<PagedResult<SurveyDto>> GetSurveysAsync(SurveyQueryParams query);
    Task<SurveyDetailDto> GetSurveyByIdAsync(Guid id);
    Task<SurveyDto> CreateSurveyAsync(CreateSurveyRequest request, Guid adminId);
    Task<SurveyDto?> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request);
    Task<bool> DeleteSurveyAsync(Guid id);
    Task<SurveyDto?> PublishSurveyAsync(Guid id);
    Task<List<string>> GenerateTokensAsync(Guid surveyId, int count, int expiryDays);
    Task<SurveyAnalyticsDto?> GetAnalyticsAsync(Guid surveyId);
    Task<PagedResult<SurveyDto>> GetPublishedSurveysAsync(PaginationParams paginationParams);
    Task<InvitationResponse> SendInvitationsAsync(SendInvitationRequest request, string baseUrl);
    Task<SurveyResponseDto> SubmitSurveyResponseAsync(SubmitSurveyResponseRequest request);
}