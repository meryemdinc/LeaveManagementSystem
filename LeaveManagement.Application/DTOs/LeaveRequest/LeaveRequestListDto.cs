using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Domain.Enums;


namespace LeaveManagement.Application.DTOs.LeaveRequest
{
    public class LeaveRequestListDto
    {
        public int Id { get; set; }
        public EmployeeDto Employee { get; set; } // Birazdan oluşturacağız
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LeaveType { get; set; }
        public LeaveStatus Status { get; set; }
    }
}
