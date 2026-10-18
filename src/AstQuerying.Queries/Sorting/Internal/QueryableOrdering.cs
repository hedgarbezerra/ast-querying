using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AstQuerying.Queries.Sorting.Internal;

/// <summary>
///     Typed wrappers for LINQ ordering that avoid reflective scanning of <see cref="Queryable" /> at runtime.
/// </summary>
internal static class QueryableOrdering
{
    private static readonly MethodInfo OrderByDefinition = GetOrderingDefinition(nameof(OrderBy), orderedSource: false);
    private static readonly MethodInfo OrderByDescendingDefinition =
        GetOrderingDefinition(nameof(OrderByDescending), orderedSource: false);
    private static readonly MethodInfo ThenByDefinition = GetOrderingDefinition(nameof(ThenBy), orderedSource: true);
    private static readonly MethodInfo ThenByDescendingDefinition =
        GetOrderingDefinition(nameof(ThenByDescending), orderedSource: true);

    private static readonly ConcurrentDictionary<(MethodInfo Definition, Type Entity, Type Key), MethodInfo>
        SpecializedMethods = new();

    /// <summary>
    ///     Applies ascending <see cref="Queryable.OrderBy{TSource,TKey}" />.
    /// </summary>
    public static IOrderedQueryable<T> OrderBy<T, TKey>(
        IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector) =>
        Queryable.OrderBy(source, keySelector);

    /// <summary>
    ///     Applies descending <see cref="Queryable.OrderByDescending{TSource,TKey}" />.
    /// </summary>
    public static IOrderedQueryable<T> OrderByDescending<T, TKey>(
        IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector) =>
        Queryable.OrderByDescending(source, keySelector);

    /// <summary>
    ///     Applies ascending <see cref="Queryable.ThenBy{TSource,TKey}" />.
    /// </summary>
    public static IOrderedQueryable<T> ThenBy<T, TKey>(
        IOrderedQueryable<T> source,
        Expression<Func<T, TKey>> keySelector) =>
        Queryable.ThenBy(source, keySelector);

    /// <summary>
    ///     Applies descending <see cref="Queryable.ThenByDescending{TSource,TKey}" />.
    /// </summary>
    public static IOrderedQueryable<T> ThenByDescending<T, TKey>(
        IOrderedQueryable<T> source,
        Expression<Func<T, TKey>> keySelector) =>
        Queryable.ThenByDescending(source, keySelector);

    /// <summary>
    ///     Applies ascending <see cref="Queryable.OrderBy{TSource,TKey}" /> using a runtime key selector.
    /// </summary>
    public static IOrderedQueryable<TEntity> OrderBy<TEntity>(
        IQueryable<TEntity> source,
        LambdaExpression keySelector) where TEntity : class =>
        Invoke(source, keySelector, OrderByDefinition);

    /// <summary>
    ///     Applies descending <see cref="Queryable.OrderByDescending{TSource,TKey}" /> using a runtime key selector.
    /// </summary>
    public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(
        IQueryable<TEntity> source,
        LambdaExpression keySelector) where TEntity : class =>
        Invoke(source, keySelector, OrderByDescendingDefinition);

    /// <summary>
    ///     Applies ascending <see cref="Queryable.ThenBy{TSource,TKey}" /> using a runtime key selector.
    /// </summary>
    public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
        IOrderedQueryable<TEntity> source,
        LambdaExpression keySelector) where TEntity : class =>
        Invoke(source, keySelector, ThenByDefinition);

    /// <summary>
    ///     Applies descending <see cref="Queryable.ThenByDescending{TSource,TKey}" /> using a runtime key selector.
    /// </summary>
    public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(
        IOrderedQueryable<TEntity> source,
        LambdaExpression keySelector) where TEntity : class =>
        Invoke(source, keySelector, ThenByDescendingDefinition);

    private static IOrderedQueryable<TEntity> Invoke<TEntity>(
        IQueryable<TEntity> source,
        LambdaExpression keySelector,
        MethodInfo definition) where TEntity : class
    {
        var entityType = typeof(TEntity);
        var keyType = keySelector.ReturnType;
        var specialized = SpecializedMethods.GetOrAdd(
            (definition, entityType, keyType),
            static key => key.Definition.MakeGenericMethod(key.Entity, key.Key));

        var funcType = typeof(Func<,>).MakeGenericType(entityType, keyType);
        var typedSelector = Expression.Lambda(funcType, keySelector.Body, keySelector.Parameters);
        return (IOrderedQueryable<TEntity>)specialized.Invoke(null, [source, typedSelector])!;
    }

    private static MethodInfo GetOrderingDefinition(string name, bool orderedSource)
    {
        foreach (var method in typeof(QueryableOrdering).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (!method.IsGenericMethodDefinition || method.Name != name)
                continue;

            var parameters = method.GetParameters();
            if (parameters.Length != 2)
                continue;

            var sourceType = parameters[0].ParameterType;
            if (!sourceType.IsGenericType)
                continue;

            var sourceDefinition = sourceType.GetGenericTypeDefinition();
            if (orderedSource && sourceDefinition == typeof(IOrderedQueryable<>))
                return method;

            if (!orderedSource && sourceDefinition == typeof(IQueryable<>))
                return method;
        }

        throw new InvalidOperationException($"Ordering method '{name}' was not found.");
    }
}
