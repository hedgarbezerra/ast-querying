using System.Linq.Expressions;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Configuration;

internal sealed class PendingNavigation
{
    public PendingNavigation(string pathPrefix, List<PathSegment> prefixSegments)
    {
        PathPrefix = pathPrefix;
        PrefixSegments = prefixSegments;
    }

    public string PathPrefix { get; }

    public List<PathSegment> PrefixSegments { get; }
}

/// <summary>
/// Collects allowed filter operator names for a scalar property.
/// </summary>
public sealed class OperatorAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds the name of <typeparamref name="T"/> to the allow list (uses <see cref="IFilterOperator.Name"/>).
    /// </summary>
    public OperatorAllowList Allow<T>() where T : IFilterOperator, new()
    {
        _names.Add(new T().Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet() => _names;
}

/// <summary>
/// Collects allowed dynamic function names for filter operands on a scalar property.
/// </summary>
public sealed class FunctionAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds the name of <typeparamref name="T"/> to the allow list (uses <see cref="IFunctionProvider.Name"/>).
    /// </summary>
    public FunctionAllowList Allow<T>() where T : IFunctionProvider, new()
    {
        _names.Add(new T().Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet() => _names;
}

/// <summary>
/// Configures operator and function allow lists for a single scalar registration draft.
/// </summary>
public sealed class FilteringBuilder
{
    private readonly RegistrationDraft _draft;

    internal FilteringBuilder(RegistrationDraft draft)
    {
        _draft = draft;
    }

    /// <summary>
    /// Restricts which filter operators may be used for this scalar property.
    /// </summary>
    public FilteringBuilder Operators(Action<OperatorAllowList> configure)
    {
        var list = new OperatorAllowList();
        configure(list);
        _draft.AllowedOperators = new HashSet<string>(list.BuildSet(), StringComparer.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>
    /// Restricts which dynamic functions may appear in operands for this scalar property.
    /// </summary>
    public FilteringBuilder Functions(Action<FunctionAllowList> configure)
    {
        var list = new FunctionAllowList();
        configure(list);
        _draft.AllowedFunctions = new HashSet<string>(list.BuildSet(), StringComparer.OrdinalIgnoreCase);
        return this;
    }
}

/// <summary>
/// Configures ascending and descending sort permissions for a scalar registration draft.
/// </summary>
public sealed class SortingBuilder
{
    private readonly RegistrationDraft _draft;

    internal SortingBuilder(RegistrationDraft draft)
    {
        _draft = draft;
    }

    /// <summary>
    /// Allows ascending sort for this scalar property.
    /// </summary>
    public SortingBuilder AllowAscending()
    {
        _draft.AllowSortAsc = true;
        return this;
    }

    /// <summary>
    /// Allows descending sort for this scalar property.
    /// </summary>
    public SortingBuilder AllowDescending()
    {
        _draft.AllowSortDesc = true;
        return this;
    }
}

/// <summary>
/// Fluent anchor returned from <c>RuleFor</c> for configuring a single member on <typeparamref name="TRootEntity"/>.
/// </summary>
public sealed class FieldRuleAnchor<TRootEntity, TMember> where TRootEntity : class
{
    private readonly QueryConfiguration<TRootEntity> _owner;
    private readonly bool _isNavigation;
    private readonly RegistrationDraft? _draft;
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
    /// Sets an API alias for a scalar property (not valid on navigation anchors).
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> HasAlias(string alias)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("HasAlias is not valid for navigation members. Configure aliases on nested rules.");

        _draft!.Alias = alias;
        return this;
    }

    /// <summary>
    /// Configures allowed operators and functions for a scalar property.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> Filtering(Action<FilteringBuilder> configure)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("Filtering is only valid for scalar members.");

        configure(new FilteringBuilder(_draft!));
        return this;
    }

    /// <summary>
    /// Configures allowed sort directions for a scalar property.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> Sorting(Action<SortingBuilder> configure)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("Sorting is only valid for scalar members.");

        configure(new SortingBuilder(_draft!));
        return this;
    }

    /// <summary>
    /// Sets the default literal for a scalar property when the client omits a value.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TMember> WithDefault(TMember value)
    {
        if (_isNavigation)
            throw new QueryConfigurationException("WithDefault is only valid for scalar members.");

        _draft!.DefaultValue = value;
        return this;
    }

    /// <summary>
    /// For navigation properties, merges nested rules from a <see cref="T:AstQuerying.Queries.Configuration.QueryConfiguration`1"/> into the parent path prefix.
    /// </summary>
    /// <typeparam name="TConfig">Nested configuration type (must have a parameterless constructor).</typeparam>
    public void UseConfiguration<TConfig>() where TConfig : class, new()
    {
        if (!_isNavigation)
            throw new QueryConfigurationException("UseConfiguration is only valid for navigation class members.");

        dynamic nested = new TConfig();
        IReadOnlyList<RegistrationDraft> innerDrafts = nested.BuildPropertyDrafts();
        foreach (var d in innerDrafts)
            _owner.ImportPrefixed(_pending!, d);
    }
}

/// <summary>
/// Fluent builder for declaring rules on collection element members.
/// </summary>
public sealed class CollectionEntityRuleBuilder<TRootEntity, TItem> where TRootEntity : class where TItem : class
{
    private readonly QueryConfiguration<TRootEntity> _owner;
    private readonly string _prefix;
    private readonly List<PathSegment> _parentSegments;
    private readonly int _collectionDepth;

    internal CollectionEntityRuleBuilder(QueryConfiguration<TRootEntity> owner, string prefix,
        List<PathSegment> parentSegments, int collectionDepth)
    {
        _owner = owner;
        _prefix = prefix;
        _parentSegments = parentSegments;
        _collectionDepth = collectionDepth;
    }

    /// <summary>
    /// Declares a rule for a member on the collection element type.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TProp> RuleFor<TProp>(Expression<Func<TItem, TProp>> expr)
    {
        return _owner.AddFieldRuleForChild<TItem, TProp>(expr, _prefix, _parentSegments, _collectionDepth);
    }

    /// <summary>
    /// Declares a nested collection rule on the collection element type.
    /// </summary>
    public CollectionEntityRuleBuilder<TRootEntity, TNested> RuleForCollection<TNested>(
        Expression<Func<TItem, IEnumerable<TNested>>> expr) where TNested : class
    {
        return _owner.AddCollectionRuleForChild<TItem, TNested>(expr, _prefix, _parentSegments, _collectionDepth);
    }
}
