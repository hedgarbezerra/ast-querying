using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.ValueObjects;
using AstQuerying.Queries.Parsing.Implementations;
using AstQuerying.Queries.Tests.Support;
using AstQuerying.Queries.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class FilterExpressionBuilderTests
{
    [Fact]
    public void BuiltPredicateFiltersQueryablePersonsByNameContains()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var builder = sp.GetRequiredService<IFilterExpressionBuilder<Person>>();
        var dto = new FilterClauseDto
        {
            Field = "Name",
            Operator = "contains",
            Value = JsonSerializer.SerializeToElement("ru")
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build(node).Compile();
        var source = new[]
        {
            new Person { Id = 1, Name = "Alice", Age = 20 },
            new Person { Id = 2, Name = "Bruno", Age = 30 },
            new Person { Id = 3, Name = "Carla", Age = 25 }
        }.AsQueryable();

        var result = source.Where(predicate).ToList();
        Assert.Single(result);
        Assert.Equal("Bruno", result[0].Name);
    }

    [Fact]
    public void BuiltPredicateCombinesAndClauses()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var builder = sp.GetRequiredService<IFilterExpressionBuilder<Person>>();
        var dto = new FilterClauseDto
        {
            And =
            [
                new FilterClauseDto
                {
                    Field = "Age",
                    Op = ">",
                    Value = JsonSerializer.SerializeToElement(22)
                },
                new FilterClauseDto
                {
                    Field = "Name",
                    Op = "=",
                    Value = JsonSerializer.SerializeToElement("Carla")
                }
            ]
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build(node).Compile();
        var source = new[]
        {
            new Person { Id = 1, Name = "Alice", Age = 30 },
            new Person { Id = 3, Name = "Carla", Age = 25 }
        }.AsQueryable();

        var result = source.Where(predicate).ToList();
        Assert.Single(result);
        Assert.Equal("Carla", result[0].Name);
    }
}
