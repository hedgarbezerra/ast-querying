using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Validation.Services;

/// <summary>
///     Default <see cref="IQueryValidator" /> implementation using registry metadata and per-field allow lists.
/// </summary>
public sealed class QueryValidator : IQueryValidator
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    ///     Initializes a new instance of <see cref="QueryValidator" />.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public QueryValidator(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public void Validate(Type entityType, QueryNode root)
    {
        var meta = _registry.GetEntity(entityType) ??
                   throw new QueryValidationException($"No query metadata registered for entity {entityType.Name}.");

        ValidateNode(root, meta);
    }

    private void ValidateNode(QueryNode node, EntityMetadata meta)
    {
        switch (node)
        {
            case AndNode a:
                foreach (var c in a.Children)
                    ValidateNode(c, meta);
                break;
            case OrNode o:
                foreach (var c in o.Children)
                    ValidateNode(c, meta);
                break;
            case NotNode n:
                ValidateNode(n.Child, meta);
                break;
            case ComparisonNode cmp:
                ValidateComparison(cmp, meta);
                break;
            default:
                throw new QueryValidationException("Unknown query node.");
        }
    }

    private void ValidateComparison(ComparisonNode cmp, EntityMetadata meta)
    {
        var prop = meta.FindByPathOrAlias(cmp.FieldPath) ??
                   throw new QueryValidationException($"Unknown field '{cmp.FieldPath}'.");

        var opKey = cmp.OperatorName;
        if (!_registry.Operators.TryGetValue(opKey, out var op))
            throw new QueryValidationException($"Unknown operator '{opKey}'.");

        if (prop.AllowedOperators is not null &&
            !prop.AllowedOperators.Contains(opKey))
            throw new QueryValidationException($"Operator '{opKey}' is not allowed for field '{cmp.FieldPath}'.");

        switch (cmp)
        {
            case MultiValueComparisonNode mv:
                ValidateMultiValueComparison(mv, prop, op, opKey);
                return;
            case SingleValueComparisonNode sv:
                ValidateSingleValueComparison(sv, prop, op, opKey);
                return;
            default:
                throw new QueryValidationException("Unknown comparison node.");
        }
    }

    private void ValidateMultiValueComparison(MultiValueComparisonNode cmp, PropertyMetadata prop, IFilterOperator op,
        string opKey)
    {
        if (op is not IMultiValueFilterOperator)
            throw new QueryValidationException($"Operator '{opKey}' does not accept multiple values.");

        if (cmp.Values.Count == 0)
            throw new QueryValidationException($"Operator '{opKey}' requires values.");

        foreach (var v in cmp.Values)
            ValidateValueNode(v, prop);
    }

    private void ValidateSingleValueComparison(SingleValueComparisonNode cmp, PropertyMetadata prop, IFilterOperator op,
        string opKey)
    {
        var leaf = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;

        if (op is IMemberOnlyFilterOperator)
        {
            if (!prop.LeafType.IsClass && Nullable.GetUnderlyingType(prop.LeafType) is null)
                throw new QueryValidationException(
                    $"Operator '{opKey}' is not valid for non-nullable value type '{prop.Path}'.");

            return;
        }

        if (op is IMultiValueFilterOperator)
            throw new QueryValidationException($"Operator '{opKey}' requires multiple values.");

        if (op is not ISingleValueFilterOperator)
            throw new QueryValidationException($"Operator '{opKey}' is not supported.");

        if (IsStringTextOperator(opKey))
        {
            if (leaf != typeof(string))
                throw new QueryValidationException($"Operator '{opKey}' requires a string field at '{prop.Path}'.");

            if (cmp.Value is null)
                throw new QueryValidationException($"Operator '{opKey}' requires value.");

            ValidateValueNode(cmp.Value, prop);
            return;
        }

        if (cmp.Value is null)
            throw new QueryValidationException($"Operator '{opKey}' requires value.");

        ValidateValueNode(cmp.Value, prop);
    }

    private static bool IsStringTextOperator(string opKey)
    {
        return string.Equals(opKey, "contains", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opKey, "startsWith", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opKey, "endsWith", StringComparison.OrdinalIgnoreCase);
    }

    private void ValidateValueNode(ValueNode node, PropertyMetadata prop)
    {
        if (node is FunctionValueNode f)
        {
            if (!_registry.Functions.TryGetValue(f.FunctionName, out var provider))
                throw new QueryValidationException($"Unknown function '{f.FunctionName}'.");

            if (prop.AllowedFunctions is not null &&
                !prop.AllowedFunctions.Contains(f.FunctionName))
                throw new QueryValidationException(
                    $"Function '{f.FunctionName}' is not enabled for field '{prop.Path}'.");

            if (!provider.SupportedTypes.Any(st =>
                {
                    var propLeafType = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;
                    return st.IsAssignableFrom(prop.LeafType) || st == propLeafType;
                }))
                throw new QueryValidationException(
                    $"Function '{f.FunctionName}' cannot be applied to field '{prop.Path}' of type {prop.LeafType.Name}.");
        }
    }
}