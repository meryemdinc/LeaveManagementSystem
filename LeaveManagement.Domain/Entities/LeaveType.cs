using LeaveManagement.Domain.Common;


namespace LeaveManagement.Domain.Entities
{
    public class LeaveType : BaseEntity
    {
        public string Name { get; set; }
        public int DefaultDays { get; set; }
    }
}
