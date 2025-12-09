using LeaveManagement.API.Models;
using LeaveManagement.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // DbUpdateException için

namespace LeaveManagement.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // İsteği bir sonraki adıma ilet (Controller'a git)
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa buraya düşer
                _logger.LogError($"Bir şeyler ters gitti: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Varsayılan hata kodu: 500 (Sunucu Hatası)
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "Sunucu hatası oluştu.";

            // Hata Tipine Göre Özelleştirme (VTYS Error Handling)
            switch (exception)
            {
                // 1. Veri Bulunamadı (404)
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundException.Message;
                    break;

                // 2. Geçersiz İstek / Validasyon (400)
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = badRequestException.Message;
                    break;

                // 3. Entity Framework / Veritabanı Hataları (SQL Hataları)
                case DbUpdateException dbException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Veritabanı işlemi sırasında hata oluştu.";

                    // Detaylı SQL hatası kontrolü (Örn: Unique Key)
                    if (dbException.InnerException != null && dbException.InnerException.Message.Contains("UniqueConstraint"))
                    {
                        message = "Bu kayıt zaten mevcut (E-posta veya Kimlik tekrarı).";
                    }
                    else if (dbException.InnerException != null)
                    {
                        // Geliştirme aşamasında detayı görelim
                        message = dbException.InnerException.Message;
                    }
                    break;

                // 4. Diğer Genel Hatalar
                default:
                    message = exception.Message; // Prod ortamında bunu gizleyip "Hata oluştu" demek daha güvenlidir.
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            }.ToString());
        }
    }
}