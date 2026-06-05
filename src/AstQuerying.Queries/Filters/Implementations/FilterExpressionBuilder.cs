using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;

namespace AstQuerying.Queries.Filters.Implementations;

/// <summary>
/// Builds LINQ <see cref="Expression{Func}"/> predicates from validated AST nodes using registry operators and functions.
/// </summary>
/// <typeparam name="TEntity">The entity root type.</typeparam>
public sealed class FilterExpressionBuilder<TEntity> : IFilterExpressionBuilder<TEntity> where TEntity : class
{
    private readonly IQueryRegistry _registry;
    private readonly IValueConverter _converter;

    /// <summary>
    /// Initializes a new instance of <see cref="FilterExpressionBuilder{TEntity}"/>.
    /// </summary>
    /// <param name="registry">Registry providing metadata, operators, and functions.</param>
    /// <param name="converter">Literal value converter.</param>
    public FilterExpressionBuilder(IQueryRegistry registry, IValueConverter converter)
    {
        _registry = registry;
        _converter = converter;
    }

    /// <inheritdoc />
    public Expression<Func<TEntity, bool>> Build(QueryNode root)
    {
        var meta = _registry.GetEntity(typeof(TEntity)) ??
                   throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");

        var param = Expression.Parameter(typeof(TEntity), "e");
        var body = BuildNode(param, meta, root);
        return Expression.Lambda<Func<TEntity, bool>>(body, param);
    }

    private Expression BuildNode(ParameterExpression root, EntityMetadata meta, QueryNode node)
    {
        return node switch
        {
            AndNode a when a.Children.Count == 0 => Expression.Constant(true),
            AndNode a => a.Children.Select(c => BuildNode(root, meta, c)).Aggregate((x, y) => Expression.AndAlso(x, y)),
            OrNode o when o.Children.Count == 0 => Expression.Constant(false),
            OrNode o => o.Children.Select(c => BuildNode(root, meta, c)).Aggregate((x, y) => Expression.OrElse(x, y)),
            NotNode n => Expression.Not(BuildNode(root, meta, n.Child)),
            ComparisonNode cmp => BuildComparison(root, meta, cmp),
            _ => throw new QueryValidationException("Unsupported node.")
        };
    }

    private Expression BuildComparison(ParameterExpression root, EntityMetadata meta, ComparisonNode cmp)
    {
        var prop = meta.FindByPathOrAlias(cmp.FieldPath) ??
                   throw new QueryValidationException($"Unknown field '{cmp.FieldPath}'.");

        if (!_registry.Operators.TryGetValue(cmp.OperatorName, out var op))
            throw new QueryValidationException($"Unknown operator '{cmp.OperatorName}'.");

        var colIdx = prop.Segments.ToList().FindIndex(s => s.IsCollection);

        Expression BuildOnLeaf(Expression leaf)
        {
            var (single, list) = BuildOperatorValues(prop, cmp);
            return op.BuildPredicate(leaf, prop.LeafType, single, list);
        }

        if (colIdx < 0)
            return BuildOnLeaf(WalkChain(root, prop, 0, prop.Segments.Count));

        var collectionExpr = WalkChain(root, prop, 0, colIdx + 1);
        var itemType = prop.Segments[colIdx].MemberType;
        var itemParam = Expression.Parameter(itemType, "i");
        var innerLeaf = WalkChain(itemParam, prop, colIdx + 1, prop.Segments.Count);
        var innerPred = BuildOnLeaf(innerLeaf);
        var lambda = Expression.Lambda(innerPred, itemParam);
        var anyMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2)
            .MakeGenericMethod(itemType);
        return Expression.Call(anyMi, collectionExpr, lambda);
    }

    private (Expression? Single, IReadOnlyList<Expression>? List) BuildOperatorValues(PropertyMetadata prop,
        ComparisonNode cmp)
    {
        var target = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;

        if (string.Equals(cmp.OperatorName, "in", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cmp.OperatorName, "notIn", StringComparison.OrdinalIgnoreCase))
        {
            if (cmp.Values is null)
                return (null, Array.Empty<Expression>());

            var list = new List<Expression>();
            foreach (var v in cmp.Values)
                list.Add(Expression.Constant(ResolveValueNode(v, prop.LeafType, prop.Path), target));

            return (null, list);
        }

        if (string.Equals(cmp.OperatorName, "isNull", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cmp.OperatorName, "isNotNull", StringComparison.OrdinalIgnoreCase))
            return (null, null);

        if (cmp.Value is null)
            throw new QueryValidationException($"Operator '{cmp.OperatorName}' requires value.");

        var val = ResolveValueNode(cmp.Value, prop.LeafType, prop.Path);
        var single = Expression.Constant(val, prop.LeafType);
        return (single, null);
    }

    private object? ResolveValueNode(ValueNode node, Type leafType, string path)
    {
        return node switch
        {
            ConstantValueNode c => _converter.ConvertToType(c.Raw, leafType, path),
            FunctionValueNode f => ResolveFunction(f.FunctionName, leafType),
            _ => throw new QueryValidationException("Unsupported value node.")
        };
    }

    private object? ResolveFunction(string name, Type leafType)
    {
        if (!_registry.Functions.TryGetValue(name, out var provider))
            throw new QueryValidationException($"Unknown function '{name}'.");

        return provider.GetValue(leafType);
    }

    private static Expression WalkChain(Expression start, PropertyMetadata meta, int from, int to)
    {
        var cur = start;
        for (var i = from; i < to; i++)
            cur = Expression.PropertyOrField(cur, meta.Segments[i].Name);

        return cur;
    }
}
