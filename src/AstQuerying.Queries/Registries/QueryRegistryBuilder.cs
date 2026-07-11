using System.Reflection;
using AstQuerying.Queries.Common.DependencyInjection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configurations;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Functions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Registries;

internal static class QueryRegistryBuilder
{
    public static QueryRegistry Build(QueryEngineOptions options, IOperatorRegistry operators,
        IFunctionRegistry functions)
    {
        ArgumentNullException.ThrowIfNull(operators);
        ArgumentNullException.ThrowIfNull(functions);

        var assemblies = new HashSet<Assembly>(options.AssembliesToScan);
        var entities = new Dictionary<Type, EntityMetadata>();
        foreach (var asm in assemblies)
            ScanQueryConfigurations(asm, entities);

        ValidateBuiltInOperatorImplementationsRegistered(operators);
        ValidateBuiltInFunctionImplementationsRegistered(functions);

        return new QueryRegistry(entities, operators, functions);
    }

    private static void ValidateBuiltInOperatorImplementationsRegistered(IOperatorRegistry registry)
    {
        var assembly = typeof(IFilterOperator).Assembly;
        var discovered = ImplementationAssemblyScanner.GetImplementationTypes<IFilterOperator>().ToHashSet();
        var registered = new HashSet<Type>(registry.ByType.Keys);
        if (discovered.SetEquals(registered))
            return;

        var missing = string.Join(", ", discovered.Except(registered).Select(t => t.FullName ?? t.Name));
        var extra = string.Join(", ", registered.Except(discovered).Select(t => t.FullName ?? t.Name));
        throw new QueryConfigurationException(
            $"Built-in filter operator types are not aligned with {nameof(QueryOperatorsRegistry)}. Missing: [{missing}]. Extra: [{extra}].",
            new QueryConfigurationDiagnostics
            {
                Kind = QueryConfigurationFailureKind.BuiltInFilterOperatorRegistrationIncomplete,
                AssemblyFullName = assembly.FullName
            });
    }

    private static void ValidateBuiltInFunctionImplementationsRegistered(IFunctionRegistry registry)
    {
        var assembly = typeof(IFunction).Assembly;
        var discovered = ImplementationAssemblyScanner.GetImplementationTypes<IFunction>().ToHashSet();
        var registered = new HashSet<Type>(registry.ByType.Keys);
        if (discovered.SetEquals(registered))
            return;

        var missing = string.Join(", ", discovered.Except(registered).Select(t => t.FullName ?? t.Name));
        var extra = string.Join(", ", registered.Except(discovered).Select(t => t.FullName ?? t.Name));
        throw new QueryConfigurationException(
            $"Built-in function types are not aligned with {nameof(QueryFunctionsRegistry)}. Missing: [{missing}]. Extra: [{extra}].",
            new QueryConfigurationDiagnostics
            {
                Kind = QueryConfigurationFailureKind.BuiltInFunctionRegistrationIncomplete,
                AssemblyFullName = assembly.FullName
            });
    }

    private static void ScanQueryConfigurations(Assembly asm, Dictionary<Type, EntityMetadata> entities)
    {
        foreach (var configType in asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            var queryConfigBase = FindQueryConfigurationBase(configType);
            if (queryConfigBase is null)
                continue;

            var entityType = queryConfigBase.GetGenericArguments()[0];

            if (entities.ContainsKey(entityType))
                throw new QueryConfigurationException(
                    $"Duplicate QueryConfiguration for entity {entityType.Name} in assembly {asm.FullName}.",
                    new QueryConfigurationDiagnostics
                    {
                        Kind = QueryConfigurationFailureKind.DuplicateQueryConfigurationForEntity,
                        EntityTypeName = entityType.Name,
                        AssemblyFullName = asm.FullName
                    });

            var metadata = BuildMetadataForConfiguration(configType, entityType);
            entities.Add(entityType, metadata);
        }
    }

    private static Type? FindQueryConfigurationBase(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(QueryConfiguration<>))
                return current;

        return null;
    }

    private static EntityMetadata BuildMetadataForConfiguration(Type configType, Type entityType)
    {
        var instance = Activator.CreateInstance(configType) ??
                       throw new QueryConfigurationException($"Cannot create instance of {configType.Name}.",
                           new QueryConfigurationDiagnostics
                           {
                               Kind = QueryConfigurationFailureKind.CannotCreateConfigurationInstance,
                               ConfigTypeName = configType.Name,
                               EntityTypeName = entityType.Name
                           });

        var method = typeof(QueryRegistryBuilder).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(m => m.Name == nameof(BuildMetadataGeneric) && m.IsGenericMethodDefinition &&
                         m.GetParameters().Length == 2);

        return (EntityMetadata)method.MakeGenericMethod(entityType).Invoke(null, new[] { configType, instance })!;
    }

    private static EntityMetadata BuildMetadataGeneric<TEntity>(Type configClrType, object configInstance)
        where TEntity : class
    {
        var cfg = (QueryConfiguration<TEntity>)configInstance;
        var drafts = cfg.BuildPropertyDrafts();
        var props = drafts.Select(ToPropertyMetadata).ToList();
        var defaultSort = cfg.GetDefaultSort();
        var defaultFilter = cfg.GetDefaultFilter();
        if (defaultSort is null || defaultSort.Count == 0)
            throw new QueryConfigurationException(
                $"QueryConfiguration '{configClrType.Name}' for entity {typeof(TEntity).Name} must call DefaultSorting with at least one field.",
                new QueryConfigurationDiagnostics
                {
                    Kind = QueryConfigurationFailureKind.DefaultSortMissing,
                    EntityTypeName = typeof(TEntity).Name,
                    ConfigTypeName = configClrType.Name
                });

        return new EntityMetadata(typeof(TEntity), props, defaultSort, defaultFilter);
    }

    private static PropertyMetadata ToPropertyMetadata(RegistrationDraft d)
    {
        return new PropertyMetadata(d.Path, d.Alias, d.LeafType, d.Segments, d.DefaultValue,
            d.AllowedOperators, d.AllowedFunctions, d.AllowSortAsc, d.AllowSortDesc);
    }
}