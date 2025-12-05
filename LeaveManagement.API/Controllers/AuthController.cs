using LeaveManagement.Application.Contracts.Identity;
using LeaveManagement.Application.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto request)
        {
            var result = await _authService.Login(request);

            if (result == null)
            {
                // Kullanıcı bulunamadı veya şifre yanlış
                return Unauthorized("Giriş bilgileri hatalı.");
            }

            return Ok(result);
        }
        [HttpPost("google-login")]
        public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] ExternalAuthDto dto)
        {
            // Sadece Google isteklerini kabul et
            if (dto.Provider != "Google") return BadRequest("Geçersiz sağlayıcı.");

            // Servise sor: Bu token geçerli mi?
            var result = await _authService.LoginWithGoogle(dto.IdToken);

            if (result == null)
            {
                return Unauthorized("Google girişi başarısız.");
            }

            return Ok(result);
        }
    }
}
