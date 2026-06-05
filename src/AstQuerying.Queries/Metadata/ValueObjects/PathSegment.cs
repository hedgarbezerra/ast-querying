namespace AstQuerying.Queries.Metadata.ValueObjects;

/// <summary>
/// Describes one hop in a dotted property path, including optional collection navigation.
/// </summary>
public sealed class PathSegment
{
    /// <summary>
    /// Initializes a new instance of <see cref="PathSegment"/>.
    /// </summary>
    /// <param name="name">The CLR member name for this hop.</param>
    /// <param name="memberType">The type exposed after this hop (element type when <paramref name="isCollection"/> is <see langword="true"/>).</param>
    /// <param name="isCollection">Whether this hop traverses into a collection element.</param>
    public PathSegment(string name, Type memberType, bool isCollection)
    {
        Name = name;
        MemberType = memberType;
        IsCollection = isCollection;
    }

    /// <summary>
    /// Gets the CLR member name for this hop.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the member or element type after this hop.
    /// </summary>
    public Type MemberType { get; }

    /// <summary>
    /// Gets a value indicating whether this hop enters a collection.
    /// </summary>
    public bool IsCollection { get; }
}
