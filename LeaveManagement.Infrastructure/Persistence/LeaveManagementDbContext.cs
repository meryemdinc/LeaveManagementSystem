using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

using Microsoft.EntityFrameworkCore.Diagnostics;
namespace LeaveManagement.Infrastructure.Persistence
{
    public class LeaveManagementDbContext: DbContext
    {
        //Constructor: options parametresini base yani DbContext sınıfına gönderir
public LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // PendingModelChangesWarning uyarısını susturuyoruz.
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bu satır, "Configurations" klasöründeki tüm IEntityTypeConfiguration 
            // implementasyonlarını (EmployeeConfiguration vb.) otomatik bulur ve uygular.
            // Tek tek eklemekle uğraşmayız.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
