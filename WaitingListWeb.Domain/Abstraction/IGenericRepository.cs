using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WaitingListWeb.Domain.Abstraction
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> Entities { get; }
        Task<T?> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, CancellationToken ct = default);
        Task<IList<T>> GetAllAsync(string? includeProperties, CancellationToken ct = default);
        Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
        Task<T?> GetByIdNoTrackingAsync(object id, CancellationToken ct = default);
        Task InsertAsync(T entity, CancellationToken ct = default);
        Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(object id, CancellationToken ct = default);
        Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
        //Task<BasePaginatedList<T>> GetPaggingAsync(IQueryable<T> query, int index, int pageSize, CancellationToken ct = default);
    }
}
