using System.Linq.Expressions;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting;

/// <summary>
///     Builds key selector expressions for sort fields against a registered entity type.
/// </summary>
public interface ISortingKeyBuilder
{
    /// <summary>
    ///     Builds a key selector for a single sort field (simple paths and single-level collection min keys).
    /// </summary>
    /// <typeparam name="TEntity">The entity root type.</typeparam>
    /// <param name="field">The sort field descriptor.</param>
    /// <returns>A lambda expression whose body reads the sort key from an entity instance.</returns>
    LambdaExpression Build<TEntity>(SortFieldDto field) where TEntity : class;
}