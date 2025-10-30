using System.Linq.Expressions;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get entity by ID with related entities
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Get all entities with related entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Find entities matching a predicate with related entities
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get first entity matching predicate with includes or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get single entity matching predicate or null (throws if multiple found)
    /// </summary>
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Check if any entity matches predicate
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);

    /// <summary>
    /// Add new entity
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Update entity
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Delete entity
    /// </summary>
    void Delete(TEntity entity);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    void DeleteRange(IEnumerable<TEntity> entities);

    // ============================================
    // PAGINATION
    // ============================================

    /// <summary>
    /// Get paginated results
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Get paginated results with filter
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null);

    /// <summary>
    /// Get paginated results with filter and sorting
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

    /// <summary>
    /// Get paginated results with filter, sorting, and includes
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Get paginated results mapped to DTO
    /// </summary>
    Task<PagedResult<TDto>> GetPagedAsync<TDto>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        Expression<Func<TEntity, TDto>> selector = null) where TDto : class;

    // ============================================
    // DTO MAPPING
    // ============================================

    /// <summary>
    /// Get entity by ID mapped to DTO
    /// </summary>
    Task<TDto?> GetByIdAsync<TDto>(Guid id, Expression<Func<TEntity, TDto>> selector) where TDto : class;

    /// <summary>
    /// Get all entities mapped to DTO
    /// </summary>
    Task<IEnumerable<TDto>> GetAllAsync<TDto>(Expression<Func<TEntity, TDto>> selector) where TDto : class;

    /// <summary>
    /// Find entities mapped to DTO
    /// </summary>
    Task<IEnumerable<TDto>> FindAsync<TDto>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TDto>> selector) where TDto : class;

    /// <summary>
    /// Get first entity mapped to DTO
    /// </summary>
    Task<TDto?> FirstOrDefaultAsync<TDto>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TDto>> selector) where TDto : class;

    // ============================================
    // ADVANCED QUERYING
    // ============================================

    /// <summary>
    /// Get queryable for custom queries (use with caution)
    /// </summary>
    IQueryable<TEntity> GetQueryable();

    /// <summary>
    /// Get queryable with filter applied
    /// </summary>
    IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> filter);

    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, params object[] parameters);
}