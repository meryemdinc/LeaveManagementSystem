using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Common.Wrappers
{
    // Application/Common/Wrappers/Response.cs
    public class Response<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public Response() { }

        // Başarılı Cevap İçin
        public Response(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }

        // Hatalı Cevap İçin
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }
    }
}
