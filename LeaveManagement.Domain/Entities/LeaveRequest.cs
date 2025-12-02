using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums; // Enum'ı durum için kullanıyoruz
using System;

namespace LeaveManagement.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }

        // --- DEĞİŞİKLİK BURADA ---
        // Artık Enum tutmuyoruz, Tablo ID'si tutuyoruz.
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }
        // Navigation Property (İlişkiyi kurmak için, şimdilik yorum satırı kalabilir veya açabilirsin)
        // public LeaveType LeaveType { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}