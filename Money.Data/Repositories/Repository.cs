using Microsoft.EntityFrameworkCore;
using Money.Api.Data;
using Money.Data.Interfaces;
using System.Linq.Expressions;

namespace Money.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity;
        }

        public async Task<T> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
            return entity;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return entities;
        }

        public async Task<List<T>> GetAllAsync(int pageIndex, int pageSize, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entities = await query.OrderBy(x => EF.Property<Guid>(x, "Id")).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return entities;
        }

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();
            return entities;
        }

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            var entities = await query.OrderBy(x => EF.Property<Guid>(x, "Id")).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return entities;
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(predicate);
            return entity;
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
