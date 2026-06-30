using System.Linq.Expressions;
using DataAccessLayer.Persistence;
using DomainLayer.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _entity;

        public GenericRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
            _entity = _context.Set<T>();
        }


        public async Task Add(T entity)
        {
            await _entity.AddAsync(entity);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var exists = await _entity.AnyAsync(predicate);
            return exists;
        }

        public void Delete(T entity)
        {
            _entity.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            var items = await _entity.AsNoTracking().ToListAsync();
            return items;
        }

        public async Task<T> GetById(int id)
        {
            var item = await _entity.FindAsync(id);
            return item;
        }

        public async Task<T> GetById(string id)
        {
            var item = await _entity.FindAsync(id);
            return item;
        }

        public async Task<T> GetById(Guid id)
        {
            var item = await _entity.FindAsync(id);
            return item;
        }

        public void Update(T entity)
        {
            _entity.Update(entity);
        }

        public async Task<bool> Delete(Expression<Func<T, bool>> predicate)
        {
            var rowsAffected = await _entity.Where(predicate).ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        public async Task<T> Find(Expression<Func<T, bool>> predicate)
        {
            return await _entity.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate)
        {
            return await _entity.Where(predicate).AsNoTracking().ToListAsync();
        }
    }
}
