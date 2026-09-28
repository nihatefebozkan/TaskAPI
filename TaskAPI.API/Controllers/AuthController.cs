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
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.Register(registerDto), logger, HttpContext, "Register");
            if (!response.Success)
            {
                return response.Error!.ErrorCode switch
                {
                    ErrorCodes.NotFound => NotFound(response),
                    ErrorCodes.BadRequest => BadRequest(response),
                    _ => StatusCode(500, response)
                };
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
            var response = await OperationExecutor.ExecuteAsync(async () => await authService.Login(loginDto), logger, HttpContext, "Login");
            if (!response.Success)
            {
                return response.Error!.ErrorCode switch
                {
                    ErrorCodes.NotFound => NotFound(response),
                    ErrorCodes.BadRequest => BadRequest(response),
                    _ => StatusCode(500, response)
                };
            }
            if (response.Result!.Result != LoginResultEnum.Successfuly)
            {
                return Unauthorized(new ResponseModel<LoginDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.Unauthorized, "Kullanıcı adı veya şifre hatalı.")
                });
            }
            return Ok(response);
        }
    }
}

