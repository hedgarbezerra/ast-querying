using AstQuerying.Queries.Ast.ValueObjects;

namespace AstQuerying.Queries.Validation.Contracts;

/// <summary>
/// Validates parsed filter AST nodes against registry metadata and per-field allow lists.
/// </summary>
public interface IQueryValidator
{
    /// <summary>
    /// Validates <paramref name="root"/> for <paramref name="entityType"/> or throws a validation exception.
    /// </summary>
    /// <param name="entityType">The entity CLR type.</param>
    /// <param name="root">The filter AST root.</param>
    void Validate(Type entityType, QueryNode root);
}
