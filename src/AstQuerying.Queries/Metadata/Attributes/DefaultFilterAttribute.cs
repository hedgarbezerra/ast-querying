namespace AstQuerying.Queries.Metadata.Attributes;

/// <summary>
///     Declares a default filter clause as JSON aligned with <see cref="Filters.Dtos.FilterClauseDto" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DefaultFilterAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of <see cref="DefaultFilterAttribute" />.
    /// </summary>
    /// <param name="json">The filter clause JSON.</param>
    public DefaultFilterAttribute(string json)
    {
        Json = json;
    }

    /// <summary>
    ///     Gets the filter clause JSON.
    /// </summary>
    public string Json { get; }
}
