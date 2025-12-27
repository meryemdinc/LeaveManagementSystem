using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Application;
using LeaveManagement.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using LeaveManagement.API.Middleware;
using LeaveManagement.Infrastructure.Hubs;
namespace LeaveManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Controller'ları ekle
            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            // JWT Authentication Ayarları
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
                };
            });

            // 2. Swagger/OpenAPI Ayarları (Swashbuckle)
            builder.Services.AddEndpointsApiExplorer();
            // SWAGGER AYARLARI (GÜNCELLENDİ)
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LeaveManagement API", Version = "v1" });

                // 1. Kilit Tanımı (Security Definition)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // 2. Kilit Gereksinimi (Security Requirement)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
            }); 

            // 3. Veritabanı Bağlantısı (PostgreSQL)
            builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 4. Katman Servislerini Ekle
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // localhost'a izin ver
    .AllowCredentials()); // SignalR için Credentials şarttır!

            // 5. HTTP İstek Hattı (Pipeline) Ayarları
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Swagger JSON dosyasının yolunu elle belirtiyoruz (Garanti yöntem)
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LeaveManagement API V1");
                });
            }
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();
            app.MapHub<NotificationHub>("/notifications"); // <-- Adresimiz bu olacak

            app.MapControllers();

            app.Run();
        }
    }
}