using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.ValueObjects;

namespace AstQuerying.Queries.Parsing.Implementations;

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
            var op = root.ResolvedOperator;
            if (string.IsNullOrWhiteSpace(op))
                throw new QueryValidationException($"Filter for field '{root.Field}' is missing operator.");

            if (string.Equals(op, "in", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(op, "notIn", StringComparison.OrdinalIgnoreCase))
            {
                if (root.Values is null || root.Values.Count == 0)
                    throw new QueryValidationException($"Operator '{op}' requires a non-empty values array.");

                var nodes = root.Values.Select(ParseValueElement).ToList();
                return new ComparisonNode(root.Field, op, null, nodes);
            }

            if (string.Equals(op, "isNull", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(op, "isNotNull", StringComparison.OrdinalIgnoreCase))
                return new ComparisonNode(root.Field, op, null, null);

            if (root.Value is null)
                throw new QueryValidationException($"Operator '{op}' requires value.");

            return new ComparisonNode(root.Field, op, ParseValueElement(root.Value.Value), null);
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
