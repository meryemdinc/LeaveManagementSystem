using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace LeaveManagement.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            // Tablo adı (Opsiyonel, EF Core zaten Employees yapar ama biz belirleyelim)
            builder.ToTable("Employees");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Property Ayarları
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50); // nvarchar(50)

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Email alanı benzersiz olmalı (Unique Index)
            // Aynı mail ile iki kişi kayıt olamaz.
            builder.HasIndex(e => e.Email).IsUnique();

            builder.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(20);
        }
    }
}
