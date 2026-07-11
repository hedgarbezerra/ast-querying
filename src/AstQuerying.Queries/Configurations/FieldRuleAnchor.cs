using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configurations.Builders;

namespace AstQuerying.Queries.Configurations;

/// <summary>
///     Fluent anchor returned from <c>RuleFor</c> for configuring a single member on <typeparamref name="TRootEntity" />.
/// </summary>
public sealed class FieldRuleAnchor<TRootEntity, TMember> where TRootEntity : class
{
    private readonly RegistrationDraft? _draft;
    private readonly bool _isNavigation;
    private readonly QueryConfiguration<TRootEntity> _owner;
    private readonly PendingNavigation? _pending;

    internal FieldRuleAnchor(QueryConfiguration<TRootEntity> owner, RegistrationDraft draft)
    {
        _owner = owner;
        _isNavigation = false;
        _draft = draft;
        _pending = null;
    }

    internal FieldRuleAnchor(QueryConfiguration<TRootEntity> owner, PendingNavigation pending)
    {
        _owner = owner;
        _isNavigation = true;
        _draft = null;
        _pending = pending;
    }

    /// <summary>
    ///     Sets an API alias for a scalar property (not valid on navigation anchors).
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> HasAlias(string alias)
    {
        if (_isNavigation)
            throw new QueryConfigurationException(
                "HasAlias is not valid for navigation members. Configure aliases on nested rules.",
                new QueryConfigurationDiagnostics { Kind = QueryConfigurationFailureKind.HasAliasInvalidOnNavigation });

        _draft!.Alias = alias;
        return this;
    }

    /// <summary>
    ///     Configures allowed operators and functions for a scalar property.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> Filtering(Action<FilteringBuilder> configure)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("Filtering is only valid for scalar members.",
                new QueryConfigurationDiagnostics { Kind = QueryConfigurationFailureKind.FilteringOnlyValidOnScalar });

        configure(new FilteringBuilder(_draft!));
        return this;
    }

    /// <summary>
    ///     Configures allowed sort directions for a scalar property.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> Sorting(Action<SortingBuilder> configure)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("Sorting is only valid for scalar members.",
                new QueryConfigurationDiagnostics { Kind = QueryConfigurationFailureKind.SortingOnlyValidOnScalar });

        configure(new SortingBuilder(_draft!));
        return this;
    }

    /// <summary>
    ///     Sets the default literal for a scalar property when the client omits a value.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> WithDefault(TMember value)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("WithDefault is only valid for scalar members.",
                new QueryConfigurationDiagnostics
                    { Kind = QueryConfigurationFailureKind.WithDefaultOnlyValidOnScalar });

        _draft!.DefaultValue = value;
        return this;
    }

    /// <summary>
    ///     For navigation properties, merges nested rules from a
    ///     <see cref="T:AstQuerying.Queries.Configurations.QueryConfiguration`1" /> into the parent path prefix.
    /// </summary>
    /// <typeparam name="TConfig">Nested configuration type (must have a parameterless constructor).</typeparam>
    public void UseConfiguration<TConfig>() where TConfig : class, new()
    {
        if (!_isNavigation)
            throw new QueryConfigurationException("UseConfiguration is only valid for navigation class members.",
                new QueryConfigurationDiagnostics
                    { Kind = QueryConfigurationFailureKind.UseConfigurationOnlyValidOnNavigationClass });

        dynamic nested = new TConfig();
        IReadOnlyList<RegistrationDraft> innerDrafts = nested.BuildPropertyDrafts();
        foreach (var d in innerDrafts)
            _owner.ImportPrefixed(_pending!, d);
    }
}