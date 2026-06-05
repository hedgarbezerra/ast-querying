using System.Linq;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Configuration.DependencyInjection;
using AstQuerying.Queries.Configuration.Discovery;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Functions.Implementations;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Registry.Implementations;

internal sealed class QueryRegistry : IQueryRegistry
{
    public QueryRegistry(
        IReadOnlyDictionary<Type, EntityMetadata> entities,
        IReadOnlyDictionary<string, IFilterOperator> operators,
        IReadOnlyDictionary<string, IFunctionProvider> functions)
    {
        Entities = entities;
        Operators = operators;
        Functions = functions;
    }

    public IReadOnlyDictionary<Type, EntityMetadata> Entities { get; }

    public IReadOnlyDictionary<string, IFilterOperator> Operators { get; }

    public IReadOnlyDictionary<string, IFunctionProvider> Functions { get; }

    public EntityMetadata? GetEntity(Type entityClrType) => Entities.GetValueOrDefault(entityClrType);
}

internal static class QueryRegistryBuilder
{
    public static QueryRegistry Build(QueryEngineOptions options)
    {
        var assemblies = QueryAssemblyScanner.EnumerateAssemblies(options).ToList();

        var entities = new Dictionary<Type, EntityMetadata>();
        foreach (var asm in assemblies)
            ScanQueryConfigurations(asm, entities);

        var operators = new Dictionary<string, IFilterOperator>(StringComparer.OrdinalIgnoreCase);
        RegisterOperators(operators, new IFilterOperator[]
        {
            new EqualOperator(),
            new NotEqualOperator(),
            new GreaterThanOperator(),
            new GreaterThanOrEqualOperator(),
            new LessThanOperator(),
            new LessThanOrEqualOperator(),
            new ContainsOperator(),
            new StartsWithOperator(),
            new EndsWithOperator(),
            new InOperator(),
            new NotInOperator(),
            new IsNullOperator(),
            new IsNotNullOperator()
        });

        foreach (var type in assemblies.SelectMany(a =>
                     QueryAssemblyScanner.GetConcreteAssignableTypes(a, typeof(IFilterOperator))))
        {
            if (type.ContainsGenericParameters)
                continue;

            if (QueryAssemblyScanner.IsBuiltInPluginImplementation(type, typeof(IFilterOperator)))
                continue;

            RegisterOperator(operators, type);
        }

        var functions = new Dictionary<string, IFunctionProvider>(StringComparer.OrdinalIgnoreCase);
        RegisterFunctions(functions, new IFunctionProvider[]
        {
            new TodayFunction(),
            new NowFunction(),
            new StartOfMonthFunction(),
            new EndOfMonthFunction(),
            new CurrentYearFunction()
        });

        foreach (var type in assemblies.SelectMany(a =>
                     QueryAssemblyScanner.GetConcreteAssignableTypes(a, typeof(IFunctionProvider))))
        {
            if (type.ContainsGenericParameters)
                continue;

            if (QueryAssemblyScanner.IsBuiltInPluginImplementation(type, typeof(IFunctionProvider)))
                continue;

            RegisterFunction(functions, type);
        }

        return new QueryRegistry(entities, operators, functions);
    }

    private static void ScanQueryConfigurations(Assembly asm, Dictionary<Type, EntityMetadata> entities)
    {
        Type? configEntity;
        foreach (var type in asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            configEntity = GetConfiguredEntityType(type);
            if (configEntity is null)
                continue;

            if (entities.ContainsKey(configEntity))
                throw new QueryConfigurationException(
                    $"Duplicate QueryConfiguration for entity {configEntity.Name} in assembly {asm.FullName}.");

            var metadata = BuildMetadataForConfiguration(type, configEntity);
            entities.Add(configEntity, metadata);
        }
    }

    private static Type? GetConfiguredEntityType(Type configType)
    {
        var t = configType.BaseType;
        while (t is not null)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(QueryConfiguration<>))
                return t.GetGenericArguments()[0];

            t = t.BaseType;
        }

        return null;
    }

    private static EntityMetadata BuildMetadataForConfiguration(Type configType, Type entityType)
    {
        var instance = Activator.CreateInstance(configType) ??
                       throw new QueryConfigurationException($"Cannot create instance of {configType.Name}.");

        var method = typeof(QueryRegistryBuilder).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.Name == nameof(BuildMetadataGeneric) && m.IsGenericMethodDefinition);

        return (EntityMetadata)method.MakeGenericMethod(entityType).Invoke(null, new[] { instance })!;
    }

    private static EntityMetadata BuildMetadataGeneric<TEntity>(object configInstance) where TEntity : class
    {
        var cfg = (QueryConfiguration<TEntity>)configInstance;
        var drafts = cfg.BuildPropertyDrafts();
        var props = drafts.Select(ToPropertyMetadata).ToList();
        var defaultSort = cfg.GetDefaultSort();
        var defaultFilter = cfg.GetDefaultFilter();
        ValidateDefaultSort(typeof(TEntity), props, defaultSort);
        return new EntityMetadata(typeof(TEntity), props, defaultSort, defaultFilter);
    }

    private static PropertyMetadata ToPropertyMetadata(RegistrationDraft d)
    {
        return new PropertyMetadata(d.Path, d.Alias, d.LeafType, d.Segments, d.DefaultValue,
            d.AllowedOperators, d.AllowedFunctions, d.AllowSortAsc, d.AllowSortDesc);
    }

    private static void ValidateDefaultSort(Type entityType, IReadOnlyList<PropertyMetadata> props,
        IReadOnlyList<SortFieldDto>? defaultSort)
    {
        if (defaultSort is null || defaultSort.Count == 0)
            return;

        var meta = new EntityMetadata(entityType, props);
        foreach (var s in defaultSort)
        {
            var p = meta.FindByPathOrAlias(s.Field) ??
                    throw new QueryConfigurationException(
                        $"Default sort references unknown field '{s.Field}' on {entityType.Name}.");

            var desc = string.Equals(s.Direction, "desc", StringComparison.OrdinalIgnoreCase);
            if (desc)
            {
                if (!p.AllowSortDescending)
                    throw new QueryConfigurationException(
                        $"Default sort cannot use descending for '{s.Field}' on {entityType.Name}.");
            }
            else if (!p.AllowSortAscending)
            {
                throw new QueryConfigurationException(
                    $"Default sort cannot use ascending for '{s.Field}' on {entityType.Name}.");
            }
        }
    }

    private static void RegisterOperators(Dictionary<string, IFilterOperator> map, IEnumerable<IFilterOperator> items)
    {
        foreach (var op in items)
            AddOrThrow(map, op.Name, op);
    }

    private static void RegisterOperator(Dictionary<string, IFilterOperator> map, Type type)
    {
        if (type.ContainsGenericParameters)
            return;

        var op = (IFilterOperator)Activator.CreateInstance(type)!;
        AddOrThrow(map, op.Name, op);
    }

    private static void RegisterFunctions(Dictionary<string, IFunctionProvider> map,
        IEnumerable<IFunctionProvider> items)
    {
        foreach (var f in items)
            AddOrThrowFunction(map, f.Name, f);
    }

    private static void RegisterFunction(Dictionary<string, IFunctionProvider> map, Type type)
    {
        if (type.ContainsGenericParameters)
            return;

        var f = (IFunctionProvider)Activator.CreateInstance(type)!;
        AddOrThrowFunction(map, f.Name, f);
    }

    private static void AddOrThrow(Dictionary<string, IFilterOperator> map, string name, IFilterOperator op)
    {
        if (map.ContainsKey(name))
            throw new QueryConfigurationException($"Duplicate filter operator name '{name}'.");

        map[name] = op;
    }

    private static void AddOrThrowFunction(Dictionary<string, IFunctionProvider> map, string name,
        IFunctionProvider f)
    {
        if (map.ContainsKey(name))
            throw new QueryConfigurationException($"Duplicate function name '{name}'.");

        map[name] = f;
    }
}
