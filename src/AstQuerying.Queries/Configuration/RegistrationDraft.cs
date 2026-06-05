using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Configuration;

internal sealed class RegistrationDraft
{
    public RegistrationDraft(string path, string? alias, Type leafType, List<PathSegment> segments)
    {
        Path = path;
        Alias = alias;
        LeafType = leafType;
        Segments = segments;
    }

    public string Path { get; }

    public string? Alias { get; set; }

    public Type LeafType { get; }

    public List<PathSegment> Segments { get; }

    public object? DefaultValue { get; set; }

    public HashSet<string>? AllowedOperators { get; set; }

    public HashSet<string>? AllowedFunctions { get; set; }

    public bool AllowSortAsc { get; set; }

    public bool AllowSortDesc { get; set; }
}
