using Microsoft.AspNetCore.Mvc;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Core.Helpers;


namespace TaskAPI.API.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
        {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
            {
            var result = await OperationExecutor.ExecuteAsync(async () => await authService.RegisterAsync(registerDto), logger, HttpContext, "Kayıt Ol");

        }
        }
}

