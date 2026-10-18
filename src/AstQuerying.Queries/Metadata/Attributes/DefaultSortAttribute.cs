namespace AstQuerying.Queries.Metadata.Attributes;

/// <summary>
///     Declares default sort fields as JSON aligned with <see cref="Sorting.Dtos.SortFieldDto" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DefaultSortAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of <see cref="DefaultSortAttribute" />.
    /// </summary>
    /// <param name="json">The sort fields JSON array.</param>
    public DefaultSortAttribute(string json)
    {
        Json = json;
    }

    /// <summary>
    ///     Gets the sort fields JSON array.
    /// </summary>
    public string Json { get; }
}
