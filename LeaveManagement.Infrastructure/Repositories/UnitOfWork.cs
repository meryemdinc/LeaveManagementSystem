using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;


namespace LeaveManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LeaveManagementDbContext _context;

        // Repository değişkenleri (Henüz oluşturulmadı)
        private ILeaveRequestRepository _leaveRequestRepository;
        private IGenericRepository<Employee> _employeeRepository;
        private IGenericRepository<LeaveType> _leaveTypeRepository;

        public UnitOfWork(LeaveManagementDbContext context)
        {
            _context = context;
        }

        // Singleton Mantığı: İstenince oluştur, varsa olanı ver.
        public ILeaveRequestRepository LeaveRequestRepository =>
            _leaveRequestRepository ??= new LeaveRequestRepository(_context);

        public IGenericRepository<Employee> EmployeeRepository =>
            _employeeRepository ??= new GenericRepository<Employee>(_context);

        public IGenericRepository<LeaveType> LeaveTypeRepository =>
            _leaveTypeRepository ??= new GenericRepository<LeaveType>(_context);

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }
    }
}