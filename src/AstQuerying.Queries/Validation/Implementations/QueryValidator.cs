using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Validation.Contracts;

namespace AstQuerying.Queries.Validation.Implementations;

/// <summary>
/// Default <see cref="IQueryValidator"/> implementation using registry metadata and per-field allow lists.
/// </summary>
public sealed class QueryValidator : IQueryValidator
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryValidator"/>.
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
        if (!_registry.Operators.TryGetValue(opKey, out _))
            throw new QueryValidationException($"Unknown operator '{opKey}'.");

        if (prop.AllowedOperators is not null &&
            !prop.AllowedOperators.Contains(opKey))
            throw new QueryValidationException($"Operator '{opKey}' is not allowed for field '{cmp.FieldPath}'.");

        var leaf = Nullable.GetUnderlyingType(prop.LeafType) ?? prop.LeafType;

        if (string.Equals(opKey, "isNull", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(opKey, "isNotNull", StringComparison.OrdinalIgnoreCase))
        {
            if (!prop.LeafType.IsClass && Nullable.GetUnderlyingType(prop.LeafType) is null)
                throw new QueryValidationException($"Operator '{opKey}' is not valid for non-nullable value type '{prop.Path}'.");

            return;
        }

        if (string.Equals(opKey, "in", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(opKey, "notIn", StringComparison.OrdinalIgnoreCase))
        {
            if (cmp.Values is null || cmp.Values.Count == 0)
                throw new QueryValidationException($"Operator '{opKey}' requires values.");

            foreach (var v in cmp.Values)
                ValidateValueNode(v, prop);

            return;
        }

        if (string.Equals(opKey, "contains", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(opKey, "startsWith", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(opKey, "endsWith", StringComparison.OrdinalIgnoreCase))
        {
            if (leaf != typeof(string))
                throw new QueryValidationException($"Operator '{opKey}' requires a string field at '{prop.Path}'.");

            if (cmp.Value is null)
                throw new QueryValidationException($"Operator '{opKey}' requires value.");
        }

        if (cmp.Value is null && cmp.Values is null)
            throw new QueryValidationException($"Operator '{opKey}' requires value or values.");

        if (cmp.Value is not null)
            ValidateValueNode(cmp.Value, prop);
    }

    private void ValidateValueNode(ValueNode node, PropertyMetadata prop)
    {
        if (node is FunctionValueNode f)
        {
            if (!_registry.Functions.TryGetValue(f.FunctionName, out _))
                throw new QueryValidationException($"Unknown function '{f.FunctionName}'.");

            if (prop.AllowedFunctions is not null &&
                !prop.AllowedFunctions.Contains(f.FunctionName))
                throw new QueryValidationException($"Function '{f.FunctionName}' is not allowed for field '{prop.Path}'.");
        }
    }
}
