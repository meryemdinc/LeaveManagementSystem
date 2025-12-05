
namespace LeaveManagement.Application.DTOs.Auth
{
    public class ExternalAuthDto
    {
        public string Provider { get; set; } // "Google"
        public string IdToken { get; set; }  // Şifre
    }
}
