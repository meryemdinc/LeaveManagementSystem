using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LeaveManagement.Infrastructure.Persistence.Configurations
{
    public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
    {
        public void Configure(EntityTypeBuilder<LeaveType> builder)
        {
            // Tablo Ayarları
            builder.HasKey(q => q.Id); // Primary Key
            builder.Property(q => q.Name).IsRequired().HasMaxLength(50); // Zorunlu ve max 50 karakter

            // --- SEED DATA (Başlangıç Verileri) ---
            // Migration çalışınca bu veriler otomatik eklenecek!
            builder.HasData(
                new LeaveType
                {
                    Id = 1,
                    Name = "Yıllık İzin",
                    DefaultDays = 14,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType
                {
                    Id = 2,
                    Name = "Mazeret İzni",
                    DefaultDays = 5,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType
                {
                    Id = 3,
                    Name = "Hastalık İzni",
                    DefaultDays = 10,
                    CreatedDate = DateTime.UtcNow
                }
            );
        }
    }
}