using LeaveManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Şifreler asla düz metin tutulmaz
        public string Role { get; set; } // "Admin" veya "Employee"

        // Navigation Property
        // Bir çalışanın birden fazla izin talebi olabilir.
        public ICollection<LeaveRequest> LeaveRequests { get; set; }
    }
}
