namespace AstQuerying.Queries.Ast.ValueObjects;

/// <summary>
/// Base type for nodes in a logical filter abstract syntax tree.
/// </summary>
public abstract record QueryNode;

/// <summary>
/// Logical conjunction of child predicates (short-circuiting AND).
/// </summary>
/// <param name="Children">Child nodes combined with AND.</param>
public sealed record AndNode(IReadOnlyList<QueryNode> Children) : QueryNode;

/// <summary>
/// Logical disjunction of child predicates (short-circuiting OR).
/// </summary>
/// <param name="Children">Child nodes combined with OR.</param>
public sealed record OrNode(IReadOnlyList<QueryNode> Children) : QueryNode;

/// <summary>Unary logical negation of a single child filter expression.</summary>
/// <param name="Child">The subtree to negate.</param>
public sealed record NotNode(QueryNode Child) : QueryNode;

/// <summary>
/// Leaf comparison of a field using a named operator and optional value(s).
/// </summary>
/// <param name="FieldPath">Property path or alias.</param>
/// <param name="OperatorName">Operator key (for example <c>=</c>, <c>in</c>).</param>
/// <param name="Value">Single operand, when applicable.</param>
/// <param name="Values">Multiple operands for set-style operators.</param>
public sealed record ComparisonNode(string FieldPath, string OperatorName, ValueNode? Value,
    IReadOnlyList<ValueNode>? Values) : QueryNode;
