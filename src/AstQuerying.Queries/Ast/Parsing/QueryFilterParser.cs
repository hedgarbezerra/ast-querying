using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Ast.Parsing;

/// <inheritdoc />
public sealed class QueryFilterParser(IOperatorRegistry operators) : IQueryFilterParser
{
    /// <inheritdoc />
    public QueryNode Parse(FilterClauseDto root)
    {
        return ParseClause(root);
    }

    private QueryNode ParseClause(FilterClauseDto root)
    {
        if (root.And is { Count: > 0 })
        {
            var children = root.And.Select(ParseClause).ToList();
            return new AndNode(children);
        }

        if (root.Or is { Count: > 0 })
        {
            var children = root.Or.Select(ParseClause).ToList();
            return new OrNode(children);
        }

        if (root.Not is not null)
            return new NotNode(ParseClause(root.Not));

        if (!string.IsNullOrWhiteSpace(root.Field))
        {
            var op = root.Operator;
            if (string.IsNullOrWhiteSpace(op))
                throw new QueryParseException($"Filter for field '{root.Field}' is missing operator.",
                    ClauseKind.MissingOperator);

            if (!operators.TryGetByName(op, out var impl))
                throw new QueryUnknownOperatorException(op, root.Field);

            var canonicalKey = impl!.Name;

            switch (impl)
            {
                case IMultiValueFilterOperator:
                    if (root.Values is null || root.Values.Count == 0)
                        throw new QueryParseException(
                            $"Operator '{canonicalKey}' requires a non-empty values array.",
                            ClauseKind.MissingValue);
                    var nodes = root.Values.Select(ParseValueElement).ToList();
                    return new MultiValueComparisonNode(root.Field, canonicalKey, nodes);
                case IMemberOnlyFilterOperator:
                    return new SingleValueComparisonNode(root.Field, canonicalKey, null);
                case ISingleValueFilterOperator:
                    if (root.Value is null)
                        throw new QueryParseException($"Operator '{canonicalKey}' requires value.",
                            ClauseKind.MissingValue);

                    return new SingleValueComparisonNode(root.Field, canonicalKey,
                        ParseValueElement(root.Value.Value));
                default:
                    throw new QueryOperatorMismatchException(canonicalKey, root.Field, OperatorMismatchKind.NotSupported,
                        $"Operator '{canonicalKey}' for field '{root.Field}' is not supported.");
            }
        }

        throw new QueryParseException("Invalid filter clause: expected field, and, or, or not.",
            ClauseKind.InvalidClause);
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
