using AstQuerying.Queries.Filters;

namespace AstQuerying.Queries.Configurations.Builders;

/// <summary>
///     Collects allowed filter operator names for a scalar property.
/// </summary>
public sealed class OperatorAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Adds the operator API token for <typeparamref name="T" /> to the allow list.
    /// </summary>
    public OperatorAllowList Allow<T>() where T : class, IFilterOperator, new()
    {
        _names.Add(typeof(T).Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet()
    {
        return _names;
    }
}