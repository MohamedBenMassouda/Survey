using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

public class AdminController(IUnitOfWork unitOfWork, IAdminService adminService) : BaseController<Admin>(unitOfWork)
{
    private readonly IAdminService _adminService = adminService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
    {
        try
        {
            var token = await _adminService.AuthenticateAsync(loginDto);

            if (token == null)
            {
                return Unauthorized(new { error = "Invalid email or password" });
            }

            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid email or password" });
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { error = "An error occurred during authentication", details = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminDto admin)
    {
        try
        {
            var result = await _adminService.CreateAdmin(admin);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new { error = "An error occurred while creating the admin", details = ex.Message });
        }
    }
}