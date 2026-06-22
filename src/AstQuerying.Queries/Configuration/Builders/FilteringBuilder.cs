namespace AstQuerying.Queries.Configuration.Builders;

/// <summary>
/// Configures operator and function allow lists for a single scalar registration draft.
/// </summary>
public sealed class FilteringBuilder
{
    private readonly RegistrationDraft _draft;

    internal FilteringBuilder(RegistrationDraft draft)
    {
        _draft = draft;
    }

    /// <summary>
    /// Restricts which filter operators may be used for this scalar property.
    /// </summary>
    public FilteringBuilder Operators(Action<OperatorAllowList> configure)
    {
        var list = new OperatorAllowList();
        configure(list);
        _draft.AllowedOperators = new HashSet<string>(list.BuildSet(), StringComparer.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>
    /// Restricts which dynamic functions may appear in operands for this scalar property.
    /// </summary>
    public FilteringBuilder Functions(Action<FunctionAllowList> configure)
    {
        var list = new FunctionAllowList();
        configure(list);
        _draft.AllowedFunctions = new HashSet<string>(list.BuildSet(), StringComparer.OrdinalIgnoreCase);
        return this;
    }
}
