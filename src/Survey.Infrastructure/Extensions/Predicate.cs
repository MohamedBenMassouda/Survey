using System.Linq.Expressions;

namespace Survey.Infrastructure.Extensions;

public static class PredicateBuilder
{
    /// <summary>
    /// Create a predicate that always returns true
    /// </summary>
    public static Expression<Func<T, bool>> True<T>() => x => true;

    /// <summary>
    /// Create a predicate that always returns false
    /// </summary>
    public static Expression<Func<T, bool>> False<T>() => x => false;

    /// <summary>
    /// Combine two predicates with AND
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.AndAlso);
    }

    /// <summary>
    /// Combine two predicates with OR
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.OrElse);
    }

    /// <summary>
    /// Negate a predicate
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(
        this Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    /// <summary>
    /// Compose two expressions using a merge function
    /// </summary>
    private static Expression<Func<T, bool>> Compose<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second,
        Func<Expression, Expression, BinaryExpression> merge)
    {
        // Replace parameters in second expression with parameters from first
        var map = first.Parameters
            .Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);

        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

        return Expression.Lambda<Func<T, bool>>(
            merge(first.Body, secondBody),
            first.Parameters);
    }

    /// <summary>
    /// Helper class for replacing parameters in expressions
    /// </summary>
    private class ParameterRebinder : ExpressionVisitor
    {
        private readonly System.Collections.Generic.Dictionary<ParameterExpression, ParameterExpression> _map;

        private ParameterRebinder(System.Collections.Generic.Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new System.Collections.Generic.Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(
            System.Collections.Generic.Dictionary<ParameterExpression, ParameterExpression> map,
            Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_map.TryGetValue(node, out ParameterExpression replacement))
            {
                node = replacement;
            }

            return base.VisitParameter(node);
        }
    }
}

/// <summary>
/// Extension methods for building dynamic expressions
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Build a contains expression for string properties
    /// </summary>
    public static Expression<Func<T, bool>> Contains<T>(
        Expression<Func<T, string>> property,
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return PredicateBuilder.True<T>();

        var constant = Expression.Constant(value, typeof(string));
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var body = Expression.Call(property.Body, containsMethod, constant);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build a starts with expression for string properties
    /// </summary>
    public static Expression<Func<T, bool>> StartsWith<T>(
        Expression<Func<T, string>> property,
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return PredicateBuilder.True<T>();

        var constant = Expression.Constant(value, typeof(string));
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var body = Expression.Call(property.Body, startsWithMethod, constant);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build an equals expression
    /// </summary>
    public static Expression<Func<T, bool>> Equals<T, TValue>(
        Expression<Func<T, TValue>> property,
        TValue value)
    {
        var constant = Expression.Constant(value, typeof(TValue));
        var body = Expression.Equal(property.Body, constant);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build a greater than expression
    /// </summary>
    public static Expression<Func<T, bool>> GreaterThan<T, TValue>(
        Expression<Func<T, TValue>> property,
        TValue value)
    {
        var constant = Expression.Constant(value, typeof(TValue));
        var body = Expression.GreaterThan(property.Body, constant);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build a less than expression
    /// </summary>
    public static Expression<Func<T, bool>> LessThan<T, TValue>(
        Expression<Func<T, TValue>> property,
        TValue value)
    {
        var constant = Expression.Constant(value, typeof(TValue));
        var body = Expression.LessThan(property.Body, constant);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build a between expression (inclusive)
    /// </summary>
    public static Expression<Func<T, bool>> Between<T, TValue>(
        Expression<Func<T, TValue>> property,
        TValue start,
        TValue end)
    {
        var startConstant = Expression.Constant(start, typeof(TValue));
        var endConstant = Expression.Constant(end, typeof(TValue));

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(property.Body, startConstant);
        var lessThanOrEqual = Expression.LessThanOrEqual(property.Body, endConstant);

        var body = Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }

    /// <summary>
    /// Build an In expression (value in list)
    /// </summary>
    public static Expression<Func<T, bool>> In<T, TValue>(
        Expression<Func<T, TValue>> property,
        params TValue[]? values)
    {
        if (values == null || values.Length == 0)
            return PredicateBuilder.False<T>();

        var constant = Expression.Constant(values);
        var containsMethod = typeof(System.Collections.Generic.IEnumerable<TValue>)
            .GetMethod("Contains", new[] { typeof(TValue) });

        var body = Expression.Call(null, containsMethod, constant, property.Body);

        return Expression.Lambda<Func<T, bool>>(body, property.Parameters);
    }
}