using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata;
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
    private readonly IFunctionRegistry _functions;
    private readonly IEntityMetadataProvider _metadata;
    private readonly IOperatorRegistry _operators;

    /// <summary>
    ///     Initializes a new instance of <see cref="FilterExpressionBuilder" />.
    /// </summary>
    /// <param name="metadata">Entity metadata provider.</param>
    /// <param name="operators">Operator registry.</param>
    /// <param name="functions">Function registry.</param>
    /// <param name="converter">Literal value converter.</param>
    public FilterExpressionBuilder(
        IEntityMetadataProvider metadata,
        IOperatorRegistry operators,
        IFunctionRegistry functions,
        IValueConverter converter)
    {
        _metadata = metadata;
        _operators = operators;
        _functions = functions;
        _converter = converter;
    }

    /// <inheritdoc />
    public Expression<Func<TEntity, bool>> Build<TEntity>(QueryNode root) where TEntity : class
    {
        var entityType = typeof(TEntity);
        var meta = _metadata.GetMetadata(entityType) ??
                   throw new QueryEntityNotQueryableException(entityType);

        var param = Expression.Parameter(typeof(TEntity), "e");
        var body = BuildNode(entityType, param, meta, root);
        return Expression.Lambda<Func<TEntity, bool>>(body, param);
    }

    private Expression BuildNode(Type entityType, ParameterExpression root, EntityMetadata meta, QueryNode node)
    {
        return node switch
        {
            AndNode a when a.Children.Count == 0 => Expression.Constant(true),
            AndNode a => a.Children.Select(c => BuildNode(entityType, root, meta, c))
                .Aggregate((x, y) => Expression.AndAlso(x, y)),
            OrNode o when o.Children.Count == 0 => Expression.Constant(false),
            OrNode o => o.Children.Select(c => BuildNode(entityType, root, meta, c))
                .Aggregate((x, y) => Expression.OrElse(x, y)),
            NotNode n => Expression.Not(BuildNode(entityType, root, meta, n.Child)),
            ComparisonNode cmp => BuildComparison(entityType, root, meta, cmp),
            _ => throw new QueryParseException("Unsupported node.", ClauseKind.UnknownNode)
        };
    }

    private Expression BuildComparison(
        Type entityType,
        ParameterExpression root,
        EntityMetadata meta,
        ComparisonNode cmp)
    {
        var prop = meta.FindByPath(cmp.FieldPath) ??
                   throw new QueryUnknownFieldException(entityType, cmp.FieldPath);

        if (!_operators.TryGetByName(cmp.OperatorName, out var op))
            throw new QueryUnknownOperatorException(cmp.OperatorName, cmp.FieldPath);

        var colIdx = prop.Segments.ToList().FindIndex(s => s.IsCollection);

        Expression BuildOnLeaf(Expression leaf)
        {
            if (op is IMemberOnlyFilterOperator memberOnly)
                return memberOnly.CreateExpression(leaf);

            if (op is IMultiValueFilterOperator multi)
            {
                if (cmp is not MultiValueComparisonNode mv)
                    throw new QueryOperatorMismatchException(cmp.OperatorName, cmp.FieldPath,
                        OperatorMismatchKind.RequiresMultipleValues,
                        $"Operator '{cmp.OperatorName}' requires multiple values.", prop.LeafType);

                return multi.CreateExpression(leaf, BuildMultiValueExpressions(entityType, prop, mv.Values, cmp.OperatorName));
            }

            if (op is ISingleValueFilterOperator singleOp)
            {
                if (cmp is not SingleValueComparisonNode sv || sv.Value is null)
                    throw new QueryOperatorMismatchException(cmp.OperatorName, cmp.FieldPath,
                        OperatorMismatchKind.RequiresValue,
                        $"Operator '{cmp.OperatorName}' requires value.", prop.LeafType);

                var val = ResolveValueNode(sv.Value, prop.LeafType, prop.Path);
                var rhs = Expression.Constant(val, prop.LeafType);
                return singleOp.CreateExpression(leaf, rhs);
            }

            throw new QueryOperatorMismatchException(cmp.OperatorName, cmp.FieldPath, OperatorMismatchKind.NotSupported,
                $"Operator '{cmp.OperatorName}' is not supported.", prop.LeafType);
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

    private IReadOnlyList<Expression> BuildMultiValueExpressions(
        Type entityType,
        PropertyMetadata prop,
        IReadOnlyList<ValueNode> values,
        string operatorName)
    {
        if (values.Count == 0)
            throw new QueryOperatorMismatchException(operatorName, prop.Path, OperatorMismatchKind.RequiresMultipleValues,
                $"Operator '{operatorName}' requires values.", prop.LeafType);

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
            _ => throw new QueryParseException("Unsupported value node.", ClauseKind.UnsupportedValueNode)
        };
    }

    private object? ResolveFunction(string name, Type leafType)
    {
        if (!_functions.TryGetByName(name, out var provider))
            throw new QueryUnknownFunctionException(name);

        return provider!.GetValue(leafType);
    }

    private static Expression WalkChain(Expression start, PropertyMetadata meta, int from, int to)
    {
        var cur = start;
        for (var i = from; i < to; i++)
            cur = Expression.PropertyOrField(cur, meta.Segments[i].Name);

        return cur;
    }
}
