using LeaveManagement.Application.Contracts.Identity;
using LeaveManagement.Application.Contracts.Identity;
using LeaveManagement.Application.DTOs.Auth;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly LeaveManagementDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(LeaveManagementDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Login(LoginDto request)
        {
            // 1. Kullanıcıyı veritabanında ara
            var user = await _context.Employees
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Kullanıcı yoksa veya şifre yanlışsa NULL dön (Hata fırlatma, controller halletsin)
            // Not: Gerçek projede şifreler Hash'lenmiş olmalıdır, şimdilik düz metin bakıyoruz.
            if (user == null || user.PasswordHash != request.Password)
            {
                return null;
            }

            // 3. Token Üret
            string token = GenerateToken(user);

            // 4. Cevabı dön
            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                Token = token
            };
        }

        private string GenerateToken(LeaveManagement.Domain.Entities.Employee user)
        {
            // Token'ın içine gömeceğimiz bilgiler (Claims)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), // Konu (Subject)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID
                new Claim(ClaimTypes.Role, user.Role) // Rolü (Admin/Employee)
            };

            // İmza Anahtarı (appsettings.json'dan okuyoruz)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token Ayarları
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: creds
            );

            // Token'ı string olarak oluştur
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
