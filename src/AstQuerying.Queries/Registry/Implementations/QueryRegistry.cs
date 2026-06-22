using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;

namespace AstQuerying.Queries.Registry.Implementations;

internal sealed class QueryRegistry(
    IReadOnlyDictionary<Type, EntityMetadata> entities,
    IReadOnlyDictionary<string, IFilterOperator> operators,
    IReadOnlyDictionary<string, IFunction> functions)
    : IQueryRegistry
{
    public IReadOnlyDictionary<Type, EntityMetadata> Entities { get; } = entities;

    public IReadOnlyDictionary<string, IFilterOperator> Operators { get; } = operators;

    public IReadOnlyDictionary<string, IFunction> Functions { get; } = functions;

    public EntityMetadata? GetEntity(Type entityClrType) => Entities.GetValueOrDefault(entityClrType);
}
