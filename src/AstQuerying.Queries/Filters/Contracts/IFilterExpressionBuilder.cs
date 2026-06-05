using System.Linq.Expressions;
using AstQuerying.Queries.Ast.ValueObjects;

namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Builds a strongly typed LINQ predicate from a parsed filter AST for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The root entity CLR type.</typeparam>
public interface IFilterExpressionBuilder<TEntity> where TEntity : class
{
    /// <summary>
    /// Converts the logical filter tree into an expression usable with <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>.
    /// </summary>
    /// <param name="root">The root of the filter AST.</param>
    /// <returns>A lambda that evaluates to <see langword="true"/> when the entity matches the filter.</returns>
    Expression<Func<TEntity, bool>> Build(QueryNode root);
}
