using System.Security.Claims;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Interfaces;

public interface IJwtService
{
    public string GenerateToken(Admin user);
    public ClaimsPrincipal ValidateToken(string token);
}