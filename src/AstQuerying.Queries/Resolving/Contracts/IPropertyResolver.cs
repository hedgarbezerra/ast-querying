using System.Linq.Expressions;

namespace AstQuerying.Queries.Resolving.Contracts;

/// <summary>
/// Resolves a configured property path or alias to a member-access expression rooted at the entity type.
/// </summary>
public interface IPropertyResolver
{
    /// <summary>
    /// Builds an expression that reads the property identified by <paramref name="pathOrAlias"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity CLR type.</typeparam>
    /// <param name="pathOrAlias">The configured path (for example <c>Department.Name</c>) or alias.</param>
    /// <returns>A member chain expression; not a full lambda.</returns>
    Expression Resolve<TEntity>(string pathOrAlias) where TEntity : class;
}
