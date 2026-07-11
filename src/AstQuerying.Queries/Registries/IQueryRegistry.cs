using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Functions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Registries;

/// <summary>
///     Provides read-only access to all query engine registrations: entity metadata, operators, and functions.
/// </summary>
public interface IQueryRegistry
{
    /// <summary>
    ///     Gets the map of entity CLR types to their query metadata, as built from
    ///     <see cref="T:AstQuerying.Queries.Configurations.QueryConfiguration`1" /> classes.
    /// </summary>
    IReadOnlyDictionary<Type, EntityMetadata> Entities { get; }

    /// <summary>
    ///     Gets filter operators keyed by operator name (case-insensitive).
    /// </summary>
    IReadOnlyDictionary<string, IFilterOperator> Operators { get; }

    /// <summary>
    ///     Gets dynamic value providers keyed by function name (case-insensitive).
    /// </summary>
    IReadOnlyDictionary<string, IFunction> Functions { get; }

    /// <summary>
    ///     Returns metadata for <paramref name="entityClrType" />, or <see langword="null" /> when the entity is not
    ///     registered.
    /// </summary>
    /// <param name="entityClrType">The CLR type of the entity root.</param>
    EntityMetadata? GetEntity(Type entityClrType);
}