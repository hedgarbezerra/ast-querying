using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Filters.Services;

/// <summary>
///     Builds LINQ <see cref="Expression{Func}" /> predicates from validated AST nodes using registry operators and
///     functions.
/// </summary>
public sealed class FilterExpressionBuilder : IFilterExpressionBuilder
{
    private readonly IValueConverter _converter;
    private readonly IQueryRegistry _registry;

    /// <summary>
    ///     Initializes a new instance of <see cref="FilterExpressionBuilder" />.
    /// </summary>
    /// <param name="registry">Registry providing metadata, operators, and functions.</param>
    /// <param name="converter">Literal value converter.</param>
    public FilterExpressionBuilder(IQueryRegistry registry, IValueConverter converter)
    {
        _registry = registry;
        _converter = converter;
    }

    /// <inheritdoc />
    public Expression<Func<TEntity, bool>> Build<TEntity>(QueryNode root) where TEntity : class
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
            if (op is IMemberOnlyFilterOperator memberOnly)
                return memberOnly.CreateExpression(leaf);

            if (op is IMultiValueFilterOperator multi)
            {
                if (cmp is not MultiValueComparisonNode mv)
                    throw new QueryValidationException($"Operator '{cmp.OperatorName}' requires multiple values.");

                return multi.CreateExpression(leaf, BuildMultiValueExpressions(prop, mv.Values, cmp.OperatorName));
            }

            if (op is ISingleValueFilterOperator singleOp)
            {
                if (cmp is not SingleValueComparisonNode sv || sv.Value is null)
                    throw new QueryValidationException($"Operator '{cmp.OperatorName}' requires value.");

                var val = ResolveValueNode(sv.Value, prop.LeafType, prop.Path);
                var rhs = Expression.Constant(val, prop.LeafType);
                return singleOp.CreateExpression(leaf, rhs);
            }

            throw new QueryValidationException($"Operator '{cmp.OperatorName}' is not supported.");
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

    private IReadOnlyList<Expression> BuildMultiValueExpressions(PropertyMetadata prop, IReadOnlyList<ValueNode> values,
        string operatorName)
    {
        if (values.Count == 0)
            throw new QueryValidationException($"Operator '{operatorName}' requires values.");

        var target = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;
        var list = new List<Expression>();
        foreach (var v in values)
            list.Add(Expression.Constant(ResolveValueNode(v, prop.LeafType, prop.Path), target));

        return list;
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