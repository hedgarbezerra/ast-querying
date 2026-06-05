using System.Linq.Expressions;

namespace AstQuerying.Queries.Resolving.Contracts;

/// <summary>
/// Resolves a configured property path or alias to a member-access expression rooted at <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity CLR type.</typeparam>
public interface IPropertyResolver<TEntity> where TEntity : class
{
    /// <summary>
    /// Builds an expression that reads the property identified by <paramref name="pathOrAlias"/>.
    /// </summary>
    /// <param name="pathOrAlias">The configured path (for example <c>Department.Name</c>) or alias.</param>
    /// <returns>A member chain expression; not a full lambda.</returns>
    Expression Resolve(string pathOrAlias);
}
