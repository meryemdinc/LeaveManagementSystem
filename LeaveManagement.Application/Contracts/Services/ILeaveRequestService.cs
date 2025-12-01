using LeaveManagement.Application.Common.Wrappers;
using LeaveManagement.Application.DTOs.LeaveRequest;

//UnitOfWork kullanarak veritabanı işlemlerini yönetiyor


namespace LeaveManagement.Application.Contracts.Services
{
    public interface ILeaveRequestService
    {
        Task<Response<int>> CreateLeaveRequest(CreateLeaveRequestDto dto);
        Task<Response<bool>> ApproveRequest(int id, bool approved);
    }
}