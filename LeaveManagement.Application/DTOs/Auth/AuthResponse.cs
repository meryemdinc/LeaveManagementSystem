using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } // İşte o giriş anahtarı!
    }
}
