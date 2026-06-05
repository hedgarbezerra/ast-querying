using System.Reflection;
using System.Linq;
using AstQuerying.Queries.Configuration.DependencyInjection;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Configuration.Discovery;

internal static class QueryAssemblyScanner
{
    public static IEnumerable<Assembly> EnumerateAssemblies(QueryEngineOptions options)
    {
        var set = new HashSet<Assembly>();
        foreach (var a in options.AssembliesToScan)
            set.Add(a);

        set.Add(typeof(QueryAssemblyScanner).Assembly);
        return set;
    }

    public static IEnumerable<Type> GetConcreteAssignableTypes(Assembly assembly, Type interfaceType)
    {
        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } &&
                                                            interfaceType.IsAssignableFrom(t)))
            yield return type;
    }

    public static bool IsBuiltInPluginImplementation(Type type, Type interfaceType)
    {
        if (type.Assembly != typeof(QueryAssemblyScanner).Assembly)
            return false;

        if (interfaceType == typeof(IFilterOperator))
            return type.Namespace == "AstQuerying.Queries.Filters.Implementations";

        if (interfaceType == typeof(IFunctionProvider))
            return type.Namespace == "AstQuerying.Queries.Functions.Implementations";

        return false;
    }

    public static IEnumerable<Type> DiscoverPluginTypes(QueryEngineOptions options, Type interfaceType)
    {
        foreach (var asm in EnumerateAssemblies(options))
        foreach (var type in GetConcreteAssignableTypes(asm, interfaceType))
        {
            if (type.ContainsGenericParameters)
                continue;

            if (IsBuiltInPluginImplementation(type, interfaceType))
                continue;

            yield return type;
        }
    }
}
