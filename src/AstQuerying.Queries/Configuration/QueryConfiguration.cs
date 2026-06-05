using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.ValueObjects;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Implementations;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Configuration;

/// <summary>
/// Base class for declaring query rules (filterable fields, sorting, defaults) for an entity type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity CLR type (reference type).</typeparam>
public abstract class QueryConfiguration<TEntity> where TEntity : class
{
    private readonly List<RegistrationDraft> _drafts = new();
    private IReadOnlyList<SortFieldDto>? _defaultSort;
    private FilterClauseDto? _defaultFilter;

    /// <summary>
    /// When implemented, registers property rules using <see cref="RuleFor{TMember}"/>, <see cref="RuleForCollection{TItem}"/>, and related helpers.
    /// </summary>
    protected abstract void Configure();

    /// <summary>
    /// Declares rules for a scalar or navigation member reachable in one member access from the current builder context.
    /// </summary>
    protected FieldRuleAnchor<TEntity, TMember> RuleFor<TMember>(Expression<Func<TEntity, TMember>> expr)
    {
        return AddFieldRuleForChild<TEntity, TMember>(expr, "", new List<PathSegment>(), 0);
    }

    /// <summary>
    /// Declares rules for elements of a collection navigation at the current level (at most one collection depth is supported).
    /// </summary>
    /// <typeparam name="TItem">The collection element type.</typeparam>
    protected CollectionEntityRuleBuilder<TEntity, TItem> RuleForCollection<TItem>(
        Expression<Func<TEntity, IEnumerable<TItem>>> expr) where TItem : class
    {
        var (name, collectionType) = MemberPathAnalyzer.RequireSingleMemberAccess(expr);
        if (!MemberPathAnalyzer.IsCollectionType(collectionType))
            throw new QueryConfigurationException($"Member '{name}' is not a collection. Use RuleFor instead.");

        var elemType = MemberPathAnalyzer.GetEnumerableElementType(collectionType);
        if (elemType != typeof(TItem))
            throw new QueryConfigurationException("Collection element type does not match RuleForCollection type argument.");

        return BeginCollection<TItem>(name, collectionType, new List<PathSegment>(), 0, "");
    }

    /// <summary>
    /// Sets the default sort specification applied when the client does not supply an explicit sort list.
    /// </summary>
    /// <param name="fields">Ordered sort fields.</param>
    protected void DefaultSorting(params SortFieldDto[] fields)
    {
        _defaultSort = fields;
    }

    /// <summary>
    /// Sets a default filter clause merged or enforced by the host when the client sends no filter.
    /// </summary>
    /// <param name="clause">The JSON filter DTO root.</param>
    protected void DefaultFilter(FilterClauseDto clause)
    {
        _defaultFilter = clause;
    }

    internal IReadOnlyList<RegistrationDraft> BuildPropertyDrafts()
    {
        _drafts.Clear();
        Configure();
        if (_drafts.Count == 0)
            throw new QueryConfigurationException($"QueryConfiguration for {typeof(TEntity).Name} must declare at least one property.");

        ValidatePropertyRoots(typeof(TEntity), _drafts);
        ValidateDrafts(typeof(TEntity), _drafts);
        return _drafts.ToList();
    }

    internal IReadOnlyList<SortFieldDto>? GetDefaultSort() => _defaultSort;

    internal FilterClauseDto? GetDefaultFilter() => _defaultFilter;

    internal void ImportPrefixed(PendingNavigation pending, RegistrationDraft child)
    {
        var newSegments = new List<PathSegment>(pending.PrefixSegments.Count + child.Segments.Count);
        newSegments.AddRange(pending.PrefixSegments);
        newSegments.AddRange(child.Segments);
        var path = pending.PathPrefix + child.Path;
        var clone = new RegistrationDraft(path, child.Alias, child.LeafType, newSegments)
        {
            DefaultValue = child.DefaultValue,
            AllowedOperators = child.AllowedOperators is null
                ? null
                : new HashSet<string>(child.AllowedOperators, StringComparer.OrdinalIgnoreCase),
            AllowedFunctions = child.AllowedFunctions is null
                ? null
                : new HashSet<string>(child.AllowedFunctions, StringComparer.OrdinalIgnoreCase),
            AllowSortAsc = child.AllowSortAsc,
            AllowSortDesc = child.AllowSortDesc
        };
        _drafts.Add(clone);
    }

    internal FieldRuleAnchor<TEntity, TProp> AddFieldRuleForChild<TChild, TProp>(Expression<Func<TChild, TProp>> expr,
        string prefix, List<PathSegment> parentSegments, int collectionDepth)
    {
        var (name, leafType) = MemberPathAnalyzer.RequireSingleMemberAccess(expr);
        if (MemberPathAnalyzer.IsCollectionType(leafType))
            throw new QueryConfigurationException($"Member '{name}' is a collection. Use RuleForCollection.");

        var isNavigation = leafType.IsClass && leafType != typeof(string) && leafType != typeof(byte[]) &&
                           !typeof(System.Collections.IEnumerable).IsAssignableFrom(leafType);

        if (isNavigation)
        {
            var segments = AppendSegment(parentSegments, name, leafType, false);
            var pending = new PendingNavigation(prefix + name + ".", segments);
            return new FieldRuleAnchor<TEntity, TProp>(this, pending);
        }

        var scalarSegments = AppendSegment(parentSegments, name, leafType, false);
        var path = prefix + name;
        var draft = new RegistrationDraft(path, null, leafType, scalarSegments);
        _drafts.Add(draft);
        return new FieldRuleAnchor<TEntity, TProp>(this, draft);
    }

    internal CollectionEntityRuleBuilder<TEntity, TNested> AddCollectionRuleForChild<TChild, TNested>(
        Expression<Func<TChild, IEnumerable<TNested>>> expr, string prefix, List<PathSegment> parentSegments,
        int collectionDepth) where TNested : class
    {
        var (name, collectionType) = MemberPathAnalyzer.RequireSingleMemberAccess(expr);
        if (!MemberPathAnalyzer.IsCollectionType(collectionType))
            throw new QueryConfigurationException($"Member '{name}' is not a collection.");

        var elemType = MemberPathAnalyzer.GetEnumerableElementType(collectionType);
        if (elemType != typeof(TNested))
            throw new QueryConfigurationException("Collection element type does not match RuleForCollection type argument.");

        return BeginCollection<TNested>(name, collectionType, parentSegments, collectionDepth, prefix);
    }

    private CollectionEntityRuleBuilder<TEntity, TItem> BeginCollection<TItem>(string name, Type collectionType,
        List<PathSegment> parentSegments, int collectionDepth, string prefix) where TItem : class
    {
        if (collectionDepth >= 1)
            throw new QueryConfigurationException("Nested ICollection paths are not supported.");

        var elemType = MemberPathAnalyzer.GetEnumerableElementType(collectionType);
        if (elemType != typeof(TItem))
            throw new QueryConfigurationException("Collection element type mismatch.");

        var segments = AppendSegment(parentSegments, name, elemType, true);
        var newPrefix = prefix + name + ".";
        return new CollectionEntityRuleBuilder<TEntity, TItem>(this, newPrefix, segments, collectionDepth + 1);
    }

    private static List<PathSegment> AppendSegment(List<PathSegment> parentSegments, string name, Type memberType,
        bool isCollection)
    {
        var copy = new List<PathSegment>(parentSegments.Count + 1);
        copy.AddRange(parentSegments);
        copy.Add(new PathSegment(name, memberType, isCollection));
        return copy;
    }

    private static void ValidatePropertyRoots(Type entityType, List<RegistrationDraft> drafts)
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in drafts)
        {
            var dot = d.Path.IndexOf('.');
            var root = dot < 0 ? d.Path : d.Path[..dot];
            roots.Add(root);
        }

        foreach (var pi in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (pi.GetIndexParameters().Length > 0)
                continue;

            if (!roots.Contains(pi.Name))
                throw new QueryConfigurationException(
                    $"Public instance property '{pi.Name}' on {entityType.Name} has no query rule. Add RuleFor or RuleForCollection.");
        }
    }

    private static void ValidateDrafts(Type entityType, IReadOnlyList<RegistrationDraft> drafts)
    {
        if (drafts.Count == 0)
            throw new QueryConfigurationException($"QueryConfiguration for {entityType.Name} must declare at least one property.");

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in drafts)
        {
            if (!paths.Add(p.Path))
                throw new QueryConfigurationException($"Duplicate property path '{p.Path}' for {entityType.Name}.");
        }

        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in drafts)
        {
            if (p.Alias is null)
                continue;

            if (!aliases.Add(p.Alias))
                throw new QueryConfigurationException($"Duplicate alias '{p.Alias}' for {entityType.Name}.");

            if (paths.Contains(p.Alias))
                throw new QueryConfigurationException($"Alias '{p.Alias}' conflicts with a path for {entityType.Name}.");
        }

        foreach (var p in drafts.Where(x => x.DefaultValue is not null))
        {
            var leaf = Nullable.GetUnderlyingType(p.LeafType) ?? p.LeafType;
            try
            {
                Convert.ChangeType(p.DefaultValue, leaf, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new QueryConfigurationException(
                    $"Default value for '{p.Path}' on {entityType.Name} is not compatible with {p.LeafType.Name}.", ex);
            }
        }
    }
}
