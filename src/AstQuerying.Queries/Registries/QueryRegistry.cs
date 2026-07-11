using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Functions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Registries;

internal sealed class QueryRegistry(
    IReadOnlyDictionary<Type, EntityMetadata> entities,
    IOperatorRegistry operators,
    IFunctionRegistry functions)
    : IQueryRegistry
{
    public IReadOnlyDictionary<Type, EntityMetadata> Entities { get; } = entities;

    public IReadOnlyDictionary<string, IFilterOperator> Operators { get; } = operators.ByName;

    public IReadOnlyDictionary<string, IFunction> Functions { get; } = functions.ByName;

    public EntityMetadata? GetEntity(Type entityClrType)
    {
        return Entities.GetValueOrDefault(entityClrType);
    }
}