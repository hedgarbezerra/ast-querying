namespace AstQuerying.Queries.Ast.ValueObjects;

/// <summary>
/// Base type for literal or dynamic values appearing in comparison nodes.
/// </summary>
public abstract record ValueNode;

/// <summary>
/// A constant literal value supplied by the client (already deserialized to CLR or JSON primitive).
/// </summary>
/// <param name="Raw">The raw constant payload.</param>
public sealed record ConstantValueNode(object? Raw) : ValueNode;

/// <summary>
/// A reference to a registered dynamic function by name (see <see cref="T:AstQuerying.Queries.Functions.Contracts.IFunctionProvider"/>).
/// </summary>
/// <param name="FunctionName">The function key from the registry.</param>
public sealed record FunctionValueNode(string FunctionName) : ValueNode;
