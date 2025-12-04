using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Infrastructure.Repositories
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(LeaveManagementDbContext context) : base(context)
        {
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsWithDetails()
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(q => q.Employee) // Employee verisini de getir (Eager Loading)
                .ToListAsync();

            return leaveRequests;
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsWithDetails(int id)
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(q => q.Employee)
                .Where(q => q.Id == id)
                .ToListAsync();

            return leaveRequests;
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsOfEmployee(int employeeId)
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(q => q.Employee)
                .Where(q => q.EmployeeId == employeeId) // FİLTRE BURADA
                .ToListAsync();
            return leaveRequests;
        }
    }
}
