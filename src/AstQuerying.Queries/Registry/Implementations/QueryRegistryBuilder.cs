using System.Linq;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configuration;
using AstQuerying.Queries.Configuration.DependencyInjection;
using AstQuerying.Queries.Configuration.Discovery;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Operators;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Functions.Implementations.Temporal;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;

namespace AstQuerying.Queries.Registry.Implementations;

internal static class QueryRegistryBuilder
{
    public static QueryRegistry Build(QueryEngineOptions options)
    {
        var assemblies = new HashSet<Assembly>(options.AssembliesToScan);
        var entities = new Dictionary<Type, EntityMetadata>();
        foreach (var asm in assemblies)
            ScanQueryConfigurations(asm, entities);

        ValidateBuiltInOperatorImplementationsRegistered();
        ValidateBuiltInFunctionImplementationsRegistered();

        return new QueryRegistry(entities, QueryOperatorsRegistry.Operators, QueryFunctionsRegistry.Functions);
    }

    private static void ValidateBuiltInOperatorImplementationsRegistered()
    {
        var assembly = typeof(IFilterOperator).Assembly;
        var discovered = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IFilterOperator).IsAssignableFrom(t))
            .ToHashSet();
        var registered = new HashSet<Type>(QueryOperatorsRegistry.Operators.Values.Select(o => o.GetType()));
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

    private static void ValidateBuiltInFunctionImplementationsRegistered()
    {
        var assembly = typeof(IFunction).Assembly;
        var discovered = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IFunction).IsAssignableFrom(t))
            .ToHashSet();
        var registered = new HashSet<Type>(QueryFunctionsRegistry.Functions.Values.Select(f => f.GetType()));
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
        foreach (var type in asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(QueryConfiguration<>).IsAssignableFrom(t)))
        {
            if (entities.ContainsKey(type))
                throw new QueryConfigurationException(
                    $"Duplicate QueryConfiguration for entity {type.Name} in assembly {asm.FullName}.",
                    new QueryConfigurationDiagnostics
                    {
                        Kind = QueryConfigurationFailureKind.DuplicateQueryConfigurationForEntity,
                        EntityTypeName = type.Name,
                        AssemblyFullName = asm.FullName
                    });

            var metadata = BuildMetadataForConfiguration(type, configEntity);
            entities.Add(type, metadata);
        }
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

        return (EntityMetadata)method.MakeGenericMethod(entityType).Invoke(null, new object[] { configType, instance })!;
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
