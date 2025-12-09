using System.Text.Json;

namespace LeaveManagement.API.Models
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        // Hata detaylarını JSON'a çeviren metot
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}