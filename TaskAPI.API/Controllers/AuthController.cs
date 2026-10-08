using Microsoft.AspNetCore.Mvc;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Models;

namespace TaskAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAccountService authService, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginDto loginDto)
        {
            var isRegistered = await MethodExecutor.ExecuteAsync(async () => await authService.Register(loginDto), logger, httpContextAccessor, "Register");
            if (!isRegistered)
                return Conflict(new ResponseModel<object> { Success = false, Result = null, Error = new ErrorModel(ErrorCodes.Conflict, "Username is Already", 409) });
            else
                return Ok(new ResponseModel<bool> { Success = true, Result = isRegistered });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var isAuthenticated = await MethodExecutor.ExecuteAsync(async () => await authService.Login(loginDto), logger, httpContextAccessor, "Login");
            if (!isAuthenticated)
                return Unauthorized(new ResponseModel<object> { Success = false, Result = null, Error = new ErrorModel(ErrorCodes.Unauthorized, "Username or Password Invalid") });
            else
                return Ok(new ResponseModel<bool> { Success = true, Result = isAuthenticated });
        }
    }
}

