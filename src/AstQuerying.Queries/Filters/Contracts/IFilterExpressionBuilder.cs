using System.Linq.Expressions;
using AstQuerying.Queries.Ast.ValueObjects;

namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Builds a strongly typed LINQ predicate from a parsed filter AST for a given entity root type.
/// </summary>
public interface IFilterExpressionBuilder
{
    /// <summary>
    /// Converts the logical filter tree into an expression usable with <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})"/>.
    /// </summary>
    /// <typeparam name="TEntity">The root entity CLR type.</typeparam>
    /// <param name="root">The root of the filter AST.</param>
    /// <returns>A lambda that evaluates to <see langword="true"/> when the entity matches the filter.</returns>
    Expression<Func<TEntity, bool>> Build<TEntity>(QueryNode root) where TEntity : class;
}
