using Microsoft.AspNetCore.Mvc;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;
using TaskAPI.Entities.Enums;


namespace TaskAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.RegisterAsync(registerDto), logger, HttpContext, "Register");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            if (response.Result == null)
            {
                return BadRequest(new ResponseModel<RegisterDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.BadRequest, "Failed to register.")
                });
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.LoginAsync(loginDto), logger, HttpContext, "Login");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            if (response.Result.Equals(LoginResultEnum.UserNotFound) || response.Result.Equals(LoginResultEnum.InvalidPassword))
            {
                return Unauthorized(new ResponseModel<LoginDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.Unauthorized, "Failed to login.")
                });
            }
            return Ok(response);
        }
    }
}

