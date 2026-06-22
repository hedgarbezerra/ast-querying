using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Registry.Implementations;

namespace AstQuerying.Queries.Ast.Parsing;

/// <summary>
/// Parses <see cref="FilterClauseDto"/> JSON trees into immutable <see cref="QueryNode"/> graphs.
/// </summary>
public static class QueryFilterParser
{
    /// <summary>
    /// Parses the root DTO into a <see cref="QueryNode"/> tree.
    /// </summary>
    /// <param name="root">The root clause; must represent AND, OR, NOT, or a leaf field comparison.</param>
    /// <returns>The parsed AST root.</returns>
    /// <exception cref="QueryValidationException">When the payload is structurally invalid.</exception>
    public static QueryNode Parse(FilterClauseDto root)
    {
        if (root.And is { Count: > 0 })
        {
            var children = root.And.Select(Parse).ToList();
            return new AndNode(children);
        }

        if (root.Or is { Count: > 0 })
        {
            var children = root.Or.Select(Parse).ToList();
            return new OrNode(children);
        }

        if (root.Not is not null)
            return new NotNode(Parse(root.Not));

        if (!string.IsNullOrWhiteSpace(root.Field))
        {
            var op = root.Operator;
            if (string.IsNullOrWhiteSpace(op))
                throw new QueryValidationException($"Filter for field '{root.Field}' is missing operator.");

            if (!QueryOperatorsRegistry.TryGetOperator(op, out var impl))
                throw new QueryValidationException($"Unknown filter operator '{op}' for field '{root.Field}'.");

            var canonicalKey = impl.Name;

            switch (impl)
            {
                case IMultiValueFilterOperator:
                    if (root.Values is null || root.Values.Count == 0)
                        throw new QueryValidationException(
                            $"Operator '{canonicalKey}' requires a non-empty values array.");

                    var nodes = root.Values.Select(ParseValueElement).ToList();
                    return new MultiValueComparisonNode(root.Field, canonicalKey, nodes);
                case IMemberOnlyFilterOperator:
                    return new SingleValueComparisonNode(root.Field, canonicalKey, null);
                case ISingleValueFilterOperator:
                    if (root.Value is null)
                        throw new QueryValidationException($"Operator '{canonicalKey}' requires value.");

                    return new SingleValueComparisonNode(root.Field, canonicalKey,
                        ParseValueElement(root.Value.Value));
                default:
                    throw new QueryValidationException(
                        $"Operator '{canonicalKey}' for field '{root.Field}' is not supported.");
            }
        }

        throw new QueryValidationException("Invalid filter clause: expected field, and, or, or not.");
    }

    private static ValueNode ParseValueElement(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.Object &&
            el.TryGetProperty("function", out var fn) &&
            fn.ValueKind == JsonValueKind.String)
            return new FunctionValueNode(fn.GetString()!);

        if (el.ValueKind == JsonValueKind.Object &&
            el.TryGetProperty("$function", out var fn2) &&
            fn2.ValueKind == JsonValueKind.String)
            return new FunctionValueNode(fn2.GetString()!);

        return new ConstantValueNode(el);
    }
}
