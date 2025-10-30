using System.Linq.Expressions;
using System.Reflection;
using Survey.Infrastructure.DTO;

namespace Survey.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string sortBy,
        bool isAscending = true)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        try
        {
            var property = GetProperty(parameter, sortBy);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = isAscending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda });
        }
        catch
        {
            // If property doesn't exist, return query unchanged
            return query;
        }
    }

    /// <summary>
    /// Apply dynamic sorting with multiple sort specifications
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        IEnumerable<SortSpec> sortSpecs)
    {
        if (sortSpecs == null || !sortSpecs.Any())
            return query;

        IOrderedQueryable<T> orderedQuery = null;
        var isFirst = true;

        foreach (var spec in sortSpecs)
        {
            if (string.IsNullOrWhiteSpace(spec.Field))
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");

            try
            {
                var property = GetProperty(parameter, spec.Field);
                var lambda = Expression.Lambda(property, parameter);

                string methodName;
                if (isFirst)
                {
                    methodName = spec.IsAscending ? "OrderBy" : "OrderByDescending";
                    isFirst = false;
                }
                else
                {
                    methodName = spec.IsAscending ? "ThenBy" : "ThenByDescending";
                }

                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type);

                orderedQuery = (IOrderedQueryable<T>)method.Invoke(null,
                    new object[] { orderedQuery ?? query, lambda });
            }
            catch
            {
                // Skip invalid property
                continue;
            }
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Apply text search across multiple properties
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params string[] properties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || properties == null || properties.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression combinedExpression = null;

        foreach (var propertyName in properties)
        {
            try
            {
                var property = GetProperty(parameter, propertyName);

                // Only search on string properties
                if (property.Type != typeof(string))
                    continue;

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchValue = Expression.Constant(searchTerm, typeof(string));
                var containsExpression = Expression.Call(property, containsMethod, searchValue);

                combinedExpression = combinedExpression == null
                    ? containsExpression
                    : Expression.OrElse(combinedExpression, containsExpression);
            }
            catch
            {
                // Skip invalid property
                continue;
            }
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Apply dynamic filter based on filter specification
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> query,
        FilterSpec filter)
    {
        if (filter == null || string.IsNullOrWhiteSpace(filter.Field))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        try
        {
            var property = GetProperty(parameter, filter.Field);
            var constant = Expression.Constant(filter.Value);

            var expression = filter.Operator switch
            {
                FilterOperator.Equals => Expression.Equal(property, constant),
                FilterOperator.NotEquals => Expression.NotEqual(property, constant),
                FilterOperator.GreaterThan => Expression.GreaterThan(property, constant),
                FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
                FilterOperator.LessThan => Expression.LessThan(property, constant),
                FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
                FilterOperator.Contains => CreateContainsExpression(property, filter.Value),
                FilterOperator.StartsWith => CreateStartsWithExpression(property, filter.Value),
                FilterOperator.EndsWith => CreateEndsWithExpression(property, filter.Value),
                _ => null
            };

            if (expression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
                query = query.Where(lambda);
            }
        }
        catch
        {
            // Skip invalid filter
        }

        return query;
    }

    /// <summary>
    /// Apply multiple filters
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        IEnumerable<FilterSpec> filters)
    {
        if (filters == null || !filters.Any())
            return query;

        foreach (var filter in filters)
        {
            query = query.ApplyFilter(filter);
        }

        return query;
    }

    /// <summary>
    /// Apply query parameters (pagination, sorting, search)
    /// </summary>
    public static IQueryable<T> ApplyQueryParams<T>(
        this IQueryable<T> query,
        QueryParams parameters,
        params string[] searchProperties)
    {
        // Apply search
        if (!string.IsNullOrWhiteSpace(parameters.Search) && searchProperties?.Length > 0)
        {
            query = query.ApplySearch(parameters.Search, searchProperties);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = query.ApplySort(parameters.SortBy, parameters.IsAscending);
        }

        return query;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private static Expression GetProperty(Expression parameter, string propertyPath)
    {
        Expression property = parameter;

        // Support nested properties (e.g., "CreatedByAdmin.Email")
        foreach (var member in propertyPath.Split('.'))
        {
            var propertyInfo = property.Type.GetProperty(
                member,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
                throw new ArgumentException($"Property '{member}' not found on type '{property.Type.Name}'");

            property = Expression.Property(property, propertyInfo);
        }

        return property;
    }

    private static Expression CreateContainsExpression(Expression property, object value)
    {
        if (property.Type != typeof(string))
            throw new InvalidOperationException("Contains operator only works with string properties");

        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var constant = Expression.Constant(value?.ToString(), typeof(string));
        return Expression.Call(property, containsMethod, constant);
    }

    private static Expression CreateStartsWithExpression(Expression property, object value)
    {
        if (property.Type != typeof(string))
            throw new InvalidOperationException("StartsWith operator only works with string properties");

        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var constant = Expression.Constant(value?.ToString(), typeof(string));
        return Expression.Call(property, startsWithMethod, constant);
    }

    private static Expression CreateEndsWithExpression(Expression property, object value)
    {
        if (property.Type != typeof(string))
            throw new InvalidOperationException("EndsWith operator only works with string properties");

        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        var constant = Expression.Constant(value?.ToString(), typeof(string));
        return Expression.Call(property, endsWithMethod, constant);
    }
}