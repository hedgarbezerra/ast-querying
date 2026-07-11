namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Structured context for a <see cref="QueryConfigurationException" /> (entity, member, duplicate keys, etc.).
/// </summary>
public sealed class QueryConfigurationDiagnostics
{
    /// <summary>
    ///     Gets or sets the failure category.
    /// </summary>
    public required QueryConfigurationFailureKind Kind { get; init; }

    /// <summary>
    ///     Gets or sets the entity CLR type name when applicable.
    /// </summary>
    public string? EntityTypeName { get; init; }

    /// <summary>
    ///     Gets or sets the <see cref="QueryConfiguration{TEntity}" /> implementation type name when applicable.
    /// </summary>
    public string? ConfigTypeName { get; init; }

    /// <summary>
    ///     Gets or sets the assembly full name when applicable.
    /// </summary>
    public string? AssemblyFullName { get; init; }

    /// <summary>
    ///     Gets or sets a simple member name from a lambda (first segment).
    /// </summary>
    public string? MemberName { get; init; }

    /// <summary>
    ///     Gets or sets a dotted property path or registration path.
    /// </summary>
    public string? PropertyPath { get; init; }

    /// <summary>
    ///     Gets or sets a duplicate operator name, function name, alias, or similar conflicting token.
    /// </summary>
    public string? ConflictingName { get; init; }

    /// <summary>
    ///     Gets or sets a default sort field token when applicable.
    /// </summary>
    public string? SortField { get; init; }

    /// <summary>
    ///     Gets or sets a default sort direction token when applicable.
    /// </summary>
    public string? SortDirection { get; init; }

    /// <summary>
    ///     Gets or sets a public property name that lacked a rule when applicable.
    /// </summary>
    public string? PropertyWithoutRule { get; init; }

    /// <summary>
    ///     Gets or sets an expected CLR type name (for example collection element type).
    /// </summary>
    public string? ExpectedTypeName { get; init; }

    /// <summary>
    ///     Gets or sets an actual CLR type name that did not match expectations.
    /// </summary>
    public string? ActualTypeName { get; init; }

    /// <summary>
    ///     Gets or sets an expression tree node type name when path analysis fails.
    /// </summary>
    public string? ExpressionNodeType { get; init; }

    /// <summary>
    ///     Gets or sets a leaf CLR type name for defaults or conversions.
    /// </summary>
    public string? LeafTypeName { get; init; }
}