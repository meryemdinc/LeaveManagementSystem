using LeaveManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Contracts.Persistence
{
    public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
    {
        Task<List<LeaveRequest>> GetLeaveRequestsWithDetails(); // Çalışan bilgisiyle beraber getir
        Task<List<LeaveRequest>> GetLeaveRequestsWithDetails(int id);

        Task<List<LeaveRequest>> GetLeaveRequestsOfEmployee(int employeeId); // İsimlendirmeyi böyle yapalım.
    }
}
