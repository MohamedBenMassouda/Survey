using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Survey.Core;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid GetCurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            throw new UnauthorizedAccessException("User ID claim not found");
        }

        return Guid.Parse(userId.Value);
    }
}