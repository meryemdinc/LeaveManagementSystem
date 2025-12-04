using LeaveManagement.Application.Contracts.Identity;
using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeaveManagement.Application.Contracts.Services; // Ekle
using LeaveManagement.Infrastructure.Services;

namespace LeaveManagement.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext ayarını Program.cs'den buraya taşıyabiliriz veya orada bırakabiliriz. 
            // Şimdilik sadece Repository'leri ekleyelim.

            // Generic Repository Kaydı
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Özel Repository Kaydı
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Auth Service Kaydı
            services.AddTransient<IAuthService, AuthService>();
            // Servis Kaydı (Bu satır yoksa ekle!)
            services.AddTransient<ILeaveRequestService, LeaveRequestService>();
            return services;
        }
    }
}
