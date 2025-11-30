using LeaveManagement.Application.Profiles;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace LeaveManagement.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // AutoMapper ayarları
            services.AddAutoMapper(cfg =>
            {
                // 1. BU SATIR ÇOK ÖNEMLİ:
                // AutoMapper'ın "GetTotal", "MaxFloat" gibi metodları otomatik bulup 
                // eşleştirmeye çalışmasını engelliyoruz. Sadece Property'leri (Ad, Soyad) eşleştirir.
                cfg.ShouldMapMethod = (m) => false;

                // 2. Profilimizi elle ekliyoruz
                cfg.AddProfile<MappingProfile>();
            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}