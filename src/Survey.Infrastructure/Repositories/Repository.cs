using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Survey.Infrastructure.Context;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Repositories;

public class Repository<TEntity>(SurveyDbContext context) : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly SurveyDbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    private const int _defaultPageSize = 10;
    private const int _defaultPageNumber = 1;
    private const int _maxPageSize = 100;

    // ============================================
    // BASIC CRUD OPERATIONS
    // ============================================

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }

        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);

        if (entity != null)
        {
            Delete(entity);
        }
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    // ============================================
    // PAGINATION
    // ============================================

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await GetPagedAsync(pageNumber, pageSize, null, null);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null)
    {
        return await GetPagedAsync(pageNumber, pageSize, filter, null);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
    {
        return await GetPagedAsync(pageNumber, pageSize, filter, orderBy, null);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[]? includes)
    {
        if (pageNumber < 1)
        {
            pageNumber = _defaultPageNumber;
        }

        if (pageSize < 1)
        {
            pageSize = _defaultPageSize;
        }

        if (pageSize > _maxPageSize)
        {
            pageSize = _maxPageSize;
        }

        IQueryable<TEntity> query = _dbSet;

        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply ordering
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TEntity>(items, totalCount, pageNumber, pageSize);
    }

    public virtual async Task<PagedResult<TDto>> GetPagedAsync<TDto>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Expression<Func<TEntity, TDto>>? selector = null) where TDto : class
    {
        // Validate parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Max page size

        IQueryable<TEntity> query = _dbSet;

        // Apply filter
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Get total count before projection
        var totalCount = await query.CountAsync();

        // Apply ordering
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Apply pagination
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        // Apply projection
        IEnumerable<TDto> items;

        if (selector != null)
        {
            items = await query.Select(selector).ToListAsync();
        }
        else
        {
            throw new InvalidOperationException(
                "Selector expression is required for DTO mapping. Please provide a selector expression.");
        }

        return new PagedResult<TDto>(items, totalCount, pageNumber, pageSize);
    }

    // ============================================
    // DTO MAPPING
    // ============================================

    public virtual async Task<TDto?> GetByIdAsync<TDto>(Guid id, Expression<Func<TEntity, TDto>> selector)
        where TDto : class
    {
        return await _dbSet
            .Where(e => e.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync<TDto>(Expression<Func<TEntity, TDto>> selector)
        where TDto : class
    {
        return await _dbSet
            .Select(selector)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<TDto>> FindAsync<TDto>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TDto>> selector) where TDto : class
    {
        return await _dbSet
            .Where(filter)
            .Select(selector)
            .ToListAsync();
    }

    public virtual async Task<TDto?> FirstOrDefaultAsync<TDto>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TDto>> selector) where TDto : class
    {
        return await _dbSet
            .Where(predicate)
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    // ============================================
    // ADVANCED QUERYING
    // ============================================

    public virtual IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public virtual IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> filter)
    {
        return _dbSet.Where(filter);
    }

    public virtual async Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }
}