// LeaveManagement.Domain/Entities/LeaveRequest.cs
using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;
using System;

namespace LeaveManagement.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } // İzin nedeni

        public LeaveType LeaveType { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending; // Varsayılan olarak beklemede

        // Foreign Key ve Navigation Property
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}