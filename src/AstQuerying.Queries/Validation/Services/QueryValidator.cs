using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Metadata;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Validation.Services;

/// <summary>
///     Default <see cref="IQueryValidator" /> implementation using generated metadata and type compatibility checks.
/// </summary>
public sealed class QueryValidator : IQueryValidator
{
    private readonly IFunctionRegistry _functions;
    private readonly IEntityMetadataProvider _metadata;
    private readonly IOperatorRegistry _operators;

    /// <summary>
    ///     Initializes a new instance of <see cref="QueryValidator" />.
    /// </summary>
    /// <param name="metadata">The entity metadata provider.</param>
    /// <param name="operators">The operator registry.</param>
    /// <param name="functions">The function registry.</param>
    public QueryValidator(
        IEntityMetadataProvider metadata,
        IOperatorRegistry operators,
        IFunctionRegistry functions)
    {
        _metadata = metadata;
        _operators = operators;
        _functions = functions;
    }

    /// <inheritdoc />
    public void Validate(Type entityType, QueryNode root)
    {
        var meta = _metadata.GetMetadata(entityType) ??
                   throw new QueryEntityNotQueryableException(entityType);

        ValidateNode(root, entityType, meta);
    }

    private void ValidateNode(QueryNode node, Type entityType, EntityMetadata meta)
    {
        switch (node)
        {
            case AndNode a:
                foreach (var c in a.Children)
                    ValidateNode(c, entityType, meta);
                break;
            case OrNode o:
                foreach (var c in o.Children)
                    ValidateNode(c, entityType, meta);
                break;
            case NotNode n:
                ValidateNode(n.Child, entityType, meta);
                break;
            case ComparisonNode cmp:
                ValidateComparison(cmp, entityType, meta);
                break;
            default:
                throw new QueryParseException("Unknown query node.", ClauseKind.UnknownNode);
        }
    }

    private void ValidateComparison(ComparisonNode cmp, Type entityType, EntityMetadata meta)
    {
        var prop = meta.FindByPath(cmp.FieldPath) ??
                   throw new QueryUnknownFieldException(entityType, cmp.FieldPath);

        var opKey = cmp.OperatorName;
        if (!_operators.TryGetByName(opKey, out var op))
            throw new QueryUnknownOperatorException(opKey, cmp.FieldPath);

        switch (cmp)
        {
            case MultiValueComparisonNode mv:
                ValidateMultiValueComparison(mv, entityType, prop, op!, opKey);
                return;
            case SingleValueComparisonNode sv:
                ValidateSingleValueComparison(sv, entityType, prop, op!, opKey);
                return;
            default:
                throw new QueryParseException("Unknown comparison node.", ClauseKind.UnknownNode);
        }
    }

    private void ValidateMultiValueComparison(
        MultiValueComparisonNode cmp,
        Type entityType,
        PropertyMetadata prop,
        IFilterOperator op,
        string opKey)
    {
        if (op is not IMultiValueFilterOperator)
            throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.RequiresMultipleValues,
                $"Operator '{opKey}' does not accept multiple values.", prop.LeafType);

        if (cmp.Values.Count == 0)
            throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.RequiresMultipleValues,
                $"Operator '{opKey}' requires values.", prop.LeafType);

        foreach (var v in cmp.Values)
            ValidateValueNode(v, entityType, prop);
    }

    private void ValidateSingleValueComparison(
        SingleValueComparisonNode cmp,
        Type entityType,
        PropertyMetadata prop,
        IFilterOperator op,
        string opKey)
    {
        var leaf = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;

        if (op is IMemberOnlyFilterOperator)
        {
            if (!prop.LeafType.IsClass && Nullable.GetUnderlyingType(prop.LeafType) is null)
                throw new QueryOperatorMismatchException(opKey, prop.Path, OperatorMismatchKind.MemberOnlyOnValueType,
                    $"Operator '{opKey}' is not valid for non-nullable value type '{prop.Path}'.", prop.LeafType);

            return;
        }

        if (op is IMultiValueFilterOperator)
            throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.RequiresMultipleValues,
                $"Operator '{opKey}' requires multiple values.", prop.LeafType);

        if (op is not ISingleValueFilterOperator)
            throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.NotSupported,
                $"Operator '{opKey}' is not supported.", prop.LeafType);

        if (IsStringTextOperator(opKey))
        {
            if (leaf != typeof(string))
                throw new QueryOperatorMismatchException(opKey, prop.Path, OperatorMismatchKind.TextOperatorRequiresString,
                    $"Operator '{opKey}' requires a string field at '{prop.Path}'.", prop.LeafType);

            if (cmp.Value is null)
                throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.RequiresValue,
                    $"Operator '{opKey}' requires value.", prop.LeafType);

            ValidateValueNode(cmp.Value, entityType, prop);
            return;
        }

        if (cmp.Value is null)
            throw new QueryOperatorMismatchException(opKey, cmp.FieldPath, OperatorMismatchKind.RequiresValue,
                $"Operator '{opKey}' requires value.", prop.LeafType);

        ValidateValueNode(cmp.Value, entityType, prop);
    }

    private static bool IsStringTextOperator(string opKey)
    {
        return string.Equals(opKey, "contains", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opKey, "startsWith", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opKey, "endsWith", StringComparison.OrdinalIgnoreCase);
    }

    private void ValidateValueNode(ValueNode node, Type entityType, PropertyMetadata prop)
    {
        if (node is FunctionValueNode f)
        {
            if (!_functions.TryGetByName(f.FunctionName, out var provider))
                throw new QueryUnknownFunctionException(f.FunctionName, prop.Path);

            if (!provider!.SupportedTypes.Any(st =>
                {
                    var propLeafType = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;
                    return st.IsAssignableFrom(prop.LeafType) || st == propLeafType;
                }))
                throw new QueryFunctionMismatchException(f.FunctionName, prop.Path, prop.LeafType);
        }
    }
}
