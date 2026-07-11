using System.Linq.Expressions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Configurations;

/// <summary>
///     Fluent builder for declaring rules on collection element members.
/// </summary>
public sealed class CollectionEntityRuleBuilder<TRootEntity, TItem> where TRootEntity : class where TItem : class
{
    private readonly int _collectionDepth;
    private readonly QueryConfiguration<TRootEntity> _owner;
    private readonly List<PathSegment> _parentSegments;
    private readonly string _prefix;

    internal CollectionEntityRuleBuilder(QueryConfiguration<TRootEntity> owner, string prefix,
        List<PathSegment> parentSegments, int collectionDepth)
    {
        _owner = owner;
        _prefix = prefix;
        _parentSegments = parentSegments;
        _collectionDepth = collectionDepth;
    }

    /// <summary>
    ///     Declares a rule for a member on the collection element type.
    /// </summary>
    public FieldRuleAnchor<TRootEntity, TProp> RuleFor<TProp>(Expression<Func<TItem, TProp>> expr)
    {
        return _owner.AddFieldRuleForChild<TItem, TProp>(expr, _prefix, _parentSegments, _collectionDepth);
    }

    /// <summary>
    ///     Declares a nested collection rule on the collection element type.
    /// </summary>
    public CollectionEntityRuleBuilder<TRootEntity, TNested> RuleForCollection<TNested>(
        Expression<Func<TItem, IEnumerable<TNested>>> expr) where TNested : class
    {
        return _owner.AddCollectionRuleForChild<TItem, TNested>(expr, _prefix, _parentSegments, _collectionDepth);
    }
}