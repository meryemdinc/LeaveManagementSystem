using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;


namespace LeaveManagement.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        // Özel Repository'ler (İçinde özel metotlar olanlar)
        ILeaveRequestRepository LeaveRequestRepository { get; }

        // Genel Repository'ler (Sadece CRUD gerekenler için Generic kullanabiliriz)
        IGenericRepository<Employee> EmployeeRepository { get; }
        IGenericRepository<LeaveType> LeaveTypeRepository { get; }

        Task<int> Save();
    }
}
