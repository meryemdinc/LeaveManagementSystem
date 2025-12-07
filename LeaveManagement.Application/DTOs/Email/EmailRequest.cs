

namespace LeaveManagement.Application.DTOs.Email
{
    public class EmailRequest
    {
        public string To { get; set; }      // Kime
        public string Subject { get; set; } // Konu
        public string Body { get; set; }    // İçerik
    }
}