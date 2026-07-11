using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Configurations;

internal sealed class PendingNavigation
{
    public PendingNavigation(string pathPrefix, List<PathSegment> prefixSegments)
    {
        PathPrefix = pathPrefix;
        PrefixSegments = prefixSegments;
    }

    public string PathPrefix { get; }

    public List<PathSegment> PrefixSegments { get; }
}