using System.Linq.Expressions;

namespace Money.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(int pageIndex, int pageSize, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);
        void Add(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}
