using LeaveManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Application.DTOs.LeaveRequest
{
    public class CreateLeaveRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
       
        public int LeaveTypeId { get; set; }
        [MaxLength(300)]
        public string Reason { get; set; }

        public int EmployeeId { get; set; } // Şimdilik manuel, ileride Token'dan alacağız
    }
}
