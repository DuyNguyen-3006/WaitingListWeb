// WaitingListWeb.Infrastructure/Implementation/GenericRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WaitingListWeb.Domain.Abstraction;
using WaitingListWeb.Infrastructure.Data;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly WaitingListDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(WaitingListDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    // Cẩn thận khi expose IQueryable — document rõ ràng
    public IQueryable<T> Entities => _dbSet.AsQueryable();

    // Find single by predicate
    public async Task<T?> FindByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }

    // Get all with optional include function (strongly-typed)
    public async Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();
        if (include != null) query = include(query);
        return await query.ToListAsync(ct);
    }

    public async Task<IList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    // Paginated list; returns custom paginated structure
    //public async Task<BasePaginatedList<T>> GetPaggingAsync(IQueryable<T> query, int index, int pageSize, CancellationToken ct = default)
    //{
    //    query = query.AsNoTracking();
    //    int count = await query.CountAsync(ct);
    //    var items = await query.Skip((index - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    //    return new BasePaginatedList<T>(items, count, index, pageSize);
    //}

    // Get by primary key (tracked)
    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    // No-tracking variant — prefer using query by key if you want NoTracking
    public async Task<T?> GetByIdNoTrackingAsync(object id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, ct);
        if (entity != null) _context.Entry(entity).State = EntityState.Detached;
        return entity;
    }

    // Insert without saving — SaveAsync is explicit (unit-of-work style)
    public async Task InsertAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _dbSet.AddAsync(entity, ct);
    }

    // Update without saving
    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    // Delete by id (without saving)
    public async Task DeleteAsync(object id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        _dbSet.Remove(entity);
    }

    // Save changes (unit-of-work)
    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task InsertRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    // Find with include string (backwards compatibility)
    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());
        }

        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<IList<T>> GetAllAsync(string? includeProperties = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProperty.Trim());
        }

        return await query.ToListAsync(ct);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(ct);
        if (entities.Any()) _dbSet.RemoveRange(entities);
    }
}
