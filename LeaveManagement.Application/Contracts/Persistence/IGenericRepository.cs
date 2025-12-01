using LeaveManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//repository interface,veritabanına nasıl erişileceğinin kurallarını belirler,
//temel CRUD işlemlerini tanımlar.
namespace LeaveManagement.Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetAll();

        Task<T> GetByIdAsync(int id);

        Task AddAsync(T entity); 
        void Update(T entity);
        void Delete(T entity);
    }
}
