namespace AstQuerying.Queries.Configuration.Builders;

/// <summary>
/// Configures ascending and descending sort permissions for a scalar registration draft.
/// </summary>
public sealed class SortingBuilder
{
    private readonly RegistrationDraft _draft;

    internal SortingBuilder(RegistrationDraft draft)
    {
        _draft = draft;
    }

    /// <summary>
    /// Allows ascending sort for this scalar property.
    /// </summary>
    public SortingBuilder AllowAscending()
    {
        _draft.AllowSortAsc = true;
        return this;
    }

    /// <summary>
    /// Allows descending sort for this scalar property.
    /// </summary>
    public SortingBuilder AllowDescending()
    {
        _draft.AllowSortDesc = true;
        return this;
    }
}
