using LeaveManagement.Application.Contracts.Persistence;
using LeaveManagement.Domain.Common;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly LeaveManagementDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(LeaveManagementDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // KRİTİK NOKTA: IQueryable ve AsNoTracking
        public IQueryable<T> GetAll()
        {
            // AsNoTracking(): Veriyi çekerken EF Core takip etmesin. 
            // Sadece okuma/listeleme yapacağımız için %50+ performans artışı sağlar.
            return _dbSet.AsNoTracking();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            // Burada SaveChanges() YOK! Onu UnitOfWork yapacak.
        }

        public void Update(T entity)
        {
            // Veritabanına gitmez, sadece nesneyi "Güncellenecek" diye işaretler.
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            // Veritabanına gitmez, sadece nesneyi "Silinecek" diye işaretler.
            _dbSet.Remove(entity);
        }
    }
}