using System.ComponentModel.DataAnnotations;

namespace Survey.Infrastructure.DTO;

public class AdminDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
}

public class CreateAdminDto
{
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}

public class AdminLoginDto
{
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginTokenDto
{
    public required string Token { get; set; }
}