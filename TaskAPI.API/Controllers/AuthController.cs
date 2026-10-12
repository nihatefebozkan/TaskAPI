using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;

namespace TaskAPI.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAccountService authService, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginRequestDto loginRequestDto)
        {
            return await MethodExecutor.ExecuteAsync<IActionResult>(
                async () =>
                {
                    var isRegistered =
                        await authService.Register(loginRequestDto);

                    if (!isRegistered)
                        throw new TaskApiException(ErrorCodes.Conflict, "Username already exists.", 409);

                    return Ok(isRegistered);

                }, logger, httpContextAccessor, nameof(Register));
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            return await MethodExecutor.ExecuteAsync<IActionResult>(
                async () =>
                {
                    var response = await authService.Login(loginRequestDto);

                    return Ok(response);

                }, logger, httpContextAccessor, nameof(Login));
        }
    }
}

