namespace AstQuerying.Queries.Metadata.Attributes;

/// <summary>
///     Marks an entity type for compile-time query metadata generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class QueryableEntityAttribute : Attribute;
