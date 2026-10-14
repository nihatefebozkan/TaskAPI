using TaskAPI.Application.Dtos;

namespace TaskAPI.Application.Interfaces
{
    public interface IAccountService
    {
        Task<bool> Register(LoginRequestDto loginRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<LoginResponseDto> Refresh(string refreshToken);
        Task<bool> Logout(string refreshToken);
    }
}
