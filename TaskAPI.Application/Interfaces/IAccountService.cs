using TaskAPI.Application.Dtos;

namespace TaskAPI.Application.Interfaces
{
    public interface IAccountService
    {
        Task<bool> Register(LoginRequestDto loginRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
    }
}
