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
    [ProducesResponseType(typeof(LoginTokenDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
    {
        var token = await _adminService.AuthenticateAsync(loginDto);
        return Ok(token);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(AdminDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateAdminDto admin)
    {
        var result = await _adminService.CreateAdmin(admin);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}