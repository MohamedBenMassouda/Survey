using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Services;

public class AdminService(IUnitOfWork unitOfWork, IJwtService jwtService) : IAdminService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<Admin> _adminRepository = unitOfWork.Repository<Admin>();
    private readonly IJwtService _jwtService = jwtService;

    public async Task<AdminDto> CreateAdmin(CreateAdminDto adminDto)
    {
        var admin = new Admin
        {
            Email = adminDto.Email,
            FullName = adminDto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDto.Password),
            IsActive = true
        };

        var createdAdmin = await _adminRepository.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return new AdminDto
        {
            Id = createdAdmin.Id,
            Email = createdAdmin.Email,
            FullName = createdAdmin.FullName
        };
    }

    public async Task<LoginTokenDto> AuthenticateAsync(AdminLoginDto loginDto)
    {
        var admin = await _adminRepository.FirstOrDefaultAsync(a => a.Email == loginDto.Email);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
        {
            throw new UnauthorizedAccessException();
        }

        var token = _jwtService.GenerateToken(admin);

        return new LoginTokenDto
        {
            Token = token
        };
    }
}