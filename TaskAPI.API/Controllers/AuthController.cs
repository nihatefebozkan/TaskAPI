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
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.RegisterAsync(registerDto), logger, HttpContext, "Register");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            if (!response.Result)
            {
                return Conflict(new ResponseModel<RegisterDto>
                {
                    Error = new Error(ErrorCodes.Conflict, "Username already exists.")
                });
            }

            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.LoginAsync(loginDto), logger, HttpContext, "Login");
            if (!response.Success)
            {
                return BadRequest(response);
            }
            if (response.Result!.Result == LoginResultEnum.UserNotFound)
            {
                return NotFound(new ResponseModel<LoginDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "User not found.")
                });
            }
            if (response.Result.Result == LoginResultEnum.InvalidPassword)
            {
                return Unauthorized(new ResponseModel<LoginDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.InvalidPassword, "Invalid password.")
                });
            }
            return Ok(response);
        }
    }
}

