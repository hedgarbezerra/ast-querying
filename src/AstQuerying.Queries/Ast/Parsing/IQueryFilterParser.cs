using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Filters.Dtos;

namespace AstQuerying.Queries.Ast.Parsing;

/// <summary>
///     Parses <see cref="FilterClauseDto" /> JSON trees into immutable <see cref="QueryNode" /> graphs.
/// </summary>
public interface IQueryFilterParser
{
    /// <summary>
    ///     Parses the root DTO into a <see cref="QueryNode" /> tree.
    /// </summary>
    /// <param name="root">The root clause; must represent AND, OR, NOT, or a leaf field comparison.</param>
    /// <returns>The parsed AST root.</returns>
    QueryNode Parse(FilterClauseDto root);
}