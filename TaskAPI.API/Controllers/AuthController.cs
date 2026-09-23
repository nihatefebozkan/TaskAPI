using Microsoft.AspNetCore.Mvc;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;


namespace TaskAPI.API.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController(IAuthService authService) : ControllerBase
        {
            [HttpPost("register")]
            public IActionResult Register(RegisterDto registerDto)
            {
                var result = authService.Register(registerDto);
                if (!result)
                return Conflict("Bu kullanıcı adı zaten kullanılıyor.");
                return StatusCode(201,"Kullanıcı başarıyla kaydedildi.");
            }
        }
}

