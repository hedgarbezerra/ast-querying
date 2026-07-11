using AstQuerying.Queries.Functions;

namespace AstQuerying.Queries.Configurations.Builders;

/// <summary>
///     Collects allowed dynamic function names for filter operands on a scalar property.
/// </summary>
public sealed class FunctionAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Adds the function API token for <typeparamref name="T" /> to the allow list.
    /// </summary>
    public FunctionAllowList Allow<T>() where T : class, IFunction, new()
    {
        _names.Add(typeof(T).Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet()
    {
        return _names;
    }
}