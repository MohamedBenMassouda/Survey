using Survey.Infrastructure.DTO;

namespace Survey.Infrastructure.Interfaces;

public interface IAdminService
{
    Task<AdminDto> CreateAdmin(CreateAdminDto adminDto);
    Task<LoginTokenDto> AuthenticateAsync(AdminLoginDto loginDto);
}