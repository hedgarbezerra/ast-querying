using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace AstQuerying.Queries.Analyzers;

internal static class PropertyPathWalker
{
    internal sealed class PropertyPathInfo
    {
        public PropertyPathInfo(string path, ITypeSymbol leafType, IReadOnlyList<SegmentInfo> segments)
        {
            Path = path;
            LeafType = leafType;
            Segments = segments;
        }

        public string Path { get; }
        public ITypeSymbol LeafType { get; }
        public IReadOnlyList<SegmentInfo> Segments { get; }
    }

    internal sealed class SegmentInfo
    {
        public SegmentInfo(string name, ITypeSymbol memberType, bool isCollection)
        {
            Name = name;
            MemberType = memberType;
            IsCollection = isCollection;
        }

        public string Name { get; }
        public ITypeSymbol MemberType { get; }
        public bool IsCollection { get; }
    }

    internal static IReadOnlyList<PropertyPathInfo> Walk(INamedTypeSymbol entityType, CancellationToken cancellationToken)
    {
        var results = new List<PropertyPathInfo>();
        WalkType(entityType, "", new List<SegmentInfo>(), 0, results, cancellationToken);
        return results;
    }

    private static void WalkType(
        INamedTypeSymbol type,
        string pathPrefix,
        List<SegmentInfo> segments,
        int collectionDepth,
        List<PropertyPathInfo> results,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var member in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic || member.IsIndexer)
                continue;

            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (member.GetMethod is null || member.GetMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            var propType = member.Type;
            var segmentName = member.Name;
            var path = string.IsNullOrEmpty(pathPrefix) ? segmentName : $"{pathPrefix}.{segmentName}";

            if (IsCollectionType(propType))
            {
                if (collectionDepth >= 1)
                    continue;

                var elementType = GetEnumerableElementType(propType);
                if (elementType is null)
                    continue;

                var collSegments = new List<SegmentInfo>(segments)
                {
                    new SegmentInfo(segmentName, elementType, true)
                };
                WalkType(elementType, path, collSegments, collectionDepth + 1, results, cancellationToken);
                continue;
            }

            if (IsScalar(propType))
            {
                var scalarSegments = new List<SegmentInfo>(segments)
                {
                    new SegmentInfo(segmentName, propType, false)
                };
                results.Add(new PropertyPathInfo(path, propType, scalarSegments));
                continue;
            }

            if (propType.TypeKind == TypeKind.Class && propType is INamedTypeSymbol namedNav)
            {
                var navSegments = new List<SegmentInfo>(segments)
                {
                    new SegmentInfo(segmentName, propType, false)
                };
                WalkType(namedNav, path, navSegments, collectionDepth, results, cancellationToken);
            }
        }
    }

    internal static bool IsCollectionType(ITypeSymbol t)
    {
        if (t.SpecialType == SpecialType.System_String)
            return false;

        if (t is IArrayTypeSymbol)
            return true;

        foreach (var i in t.AllInterfaces)
        {
            if (i is INamedTypeSymbol { IsGenericType: true } named &&
                named.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_ICollection_T)
                return true;
        }

        return false;
    }

    internal static INamedTypeSymbol? GetEnumerableElementType(ITypeSymbol t)
    {
        if (t is IArrayTypeSymbol array)
            return array.ElementType as INamedTypeSymbol;

        foreach (var i in t.AllInterfaces.Concat(t is INamedTypeSymbol n ? new[] { n } : Array.Empty<INamedTypeSymbol>()))
        {
            if (i is INamedTypeSymbol { IsGenericType: true } named &&
                named.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return named.TypeArguments[0] as INamedTypeSymbol;
            }
        }

        return null;
    }

    private static bool IsScalar(ITypeSymbol t)
    {
        var effective = UnwrapNullable(t);

        if (effective.TypeKind == TypeKind.Enum)
            return true;

        if (effective.IsValueType && effective.SpecialType != SpecialType.None)
            return true;

        return effective.SpecialType is SpecialType.System_String
            or SpecialType.System_Decimal
            or SpecialType.System_DateTime
            || effective.Name == "Guid"
            || effective.Name == "TimeSpan"
            || effective.Name == "DateOnly"
            || effective.Name == "DateTimeOffset";
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named)
            return named.TypeArguments[0];

        return t;
    }

    internal static string GetTypeDisplayName(ITypeSymbol t)
    {
        return t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
