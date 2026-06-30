using System.Linq.Expressions;

namespace DomainLayer.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetById(int id);
        Task<T> GetById(string id);
        Task<T> GetById(Guid id);
        Task Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAll();
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        public Task<bool> Delete(Expression<Func<T, bool>> predicate);
        Task<T> Find(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> predicate);
    }
}
