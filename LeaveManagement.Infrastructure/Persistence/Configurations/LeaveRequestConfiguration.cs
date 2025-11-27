using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations
{
   public class LeaveRequestConfiguration:IEntityTypeConfiguration<LeaveRequest>
    {
        public void Configure(EntityTypeBuilder<LeaveRequest> builder)
        {
            builder.ToTable("LeaveRequests");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Reason)
                .IsRequired()
                .HasMaxLength(500); // Açıklama en fazla 500 karakter

            // İlişki Tanımı (One-to-Many)
            // Bir çalışanın çok izni olabilir, bir izin bir çalışana aittir.
            builder.HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);//üzerinde izin kaydı olan çalışan silinmeye çalışıldığında çalışan silinemesin
        }
    }
}
