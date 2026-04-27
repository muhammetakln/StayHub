using Core.Abstracts.Bases;
using System.Linq.Expressions;

namespace Core.Abstracts.IRepositories
{
    public interface IRepositories<T> where T:BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync(params string[]includes);
        Task<T?> GetByIdAsync(int id, params string[] includes);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>>? filter = null, params string[] includes);
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T,bool>>? filter= null, params string[] includes);
        Task<(IEnumerable<T> Items,int TotalCount)> GetPagedAsync(int pageNumber, int pageSize,Expression<Func<T,bool>>?filter=null,
            Expression<Func<T,object>>? orderBy=null, params string[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task PermanentDeleteAsync(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        Task<int> CountAsync(Expression<Func<T, bool>>? filter=null);
    }
}
