using Core.Abstracts.Bases;
using Core.Abstracts.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories 
{
    public class Repository<T> : IRepositories<T> where T : BaseEntity
    {
        protected readonly DbContext _db;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext db)
        {
            _db = db; ;
            _dbSet = _db.Set<T>();
        }


        public async Task<IEnumerable<T>> GetAllAsync(params string[] includes)
        {
            IQueryable<T> query = _dbSet;
            query = ApplyIncludes(query, includes);
            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id, params string[] includes)
        {
            IQueryable<T> query = _dbSet;
            query = ApplyIncludes(query, includes);

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<T?> GetFirstAsync(Expression<Func<T, bool>>? filter = null, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            query = ApplyIncludes(query, includes);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>>? filter = null, params string[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            query = ApplyIncludes(query, includes);
            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            params string[] includes)
        {
            IQueryable<T> query = _dbSet;

          
            if (filter != null)
                query = query.Where(filter);

            
            query = ApplyIncludes(query, includes);

            int totalCount = await query.CountAsync();

            if (orderBy != null)
                query = query.OrderBy(orderBy);

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
           
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);

            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public Task PermanentDeleteAsync(T entity)
        {
           
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

       

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter != null)
                return await _dbSet.CountAsync(filter);

            return await _dbSet.CountAsync();
        }

        private IQueryable<T> ApplyIncludes(IQueryable<T> query, string[] includes)
        {
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }
    }
}