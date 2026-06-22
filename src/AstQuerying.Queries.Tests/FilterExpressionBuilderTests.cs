using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Ast.Parsing;
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
        var builder = sp.GetRequiredService<IFilterExpressionBuilder>();
        var dto = new FilterClauseDto
        {
            Field = "Name",
            Operator = "contains",
            Value = JsonSerializer.SerializeToElement("ru")
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build<Person>(node).Compile();
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
        var builder = sp.GetRequiredService<IFilterExpressionBuilder>();
        var dto = new FilterClauseDto
        {
            And =
            [
                new FilterClauseDto
                {
                    Field = "Age",
                    Operator = ">",
                    Value = JsonSerializer.SerializeToElement(22)
                },
                new FilterClauseDto
                {
                    Field = "Name",
                    Operator = "=",
                    Value = JsonSerializer.SerializeToElement("Carla")
                }
            ]
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build<Person>(node).Compile();
        var source = new[]
        {
            new Person { Id = 1, Name = "Alice", Age = 30 },
            new Person { Id = 3, Name = "Carla", Age = 25 }
        }.AsQueryable();

        var result = source.Where(predicate).ToList();
        Assert.Single(result);
        Assert.Equal("Carla", result[0].Name);
    }

    [Fact]
    public void BuiltPredicateFiltersByLegalAgeFunctionOnAge()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var builder = sp.GetRequiredService<IFilterExpressionBuilder>();
        var dto = new FilterClauseDto
        {
            Field = "Age",
            Operator = ">=",
            Value = JsonSerializer.SerializeToElement(new { function = "LegalAge" })
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build<Person>(node).Compile();
        var source = new[]
        {
            new Person { Id = 1, Name = "Minor", Age = 16 },
            new Person { Id = 2, Name = "Adult", Age = 30 }
        }.AsQueryable();

        var result = source.Where(predicate).ToList();
        Assert.Single(result);
        Assert.Equal("Adult", result[0].Name);
    }

    [Fact]
    public void BuiltPredicateFiltersByMinimumWageFunctionOnSalary()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var builder = sp.GetRequiredService<IFilterExpressionBuilder>();
        var dto = new FilterClauseDto
        {
            Field = "Salary",
            Operator = ">=",
            Value = JsonSerializer.SerializeToElement(new { function = "MinimumWage" })
        };
        var node = QueryFilterParser.Parse(dto);
        validator.Validate(typeof(Person), node);

        var predicate = builder.Build<Person>(node).Compile();
        var source = new[]
        {
            new Person { Id = 1, Name = "Low", Salary = 1000m },
            new Person { Id = 2, Name = "Ok", Salary = 2000m }
        }.AsQueryable();

        var result = source.Where(predicate).ToList();
        Assert.Single(result);
        Assert.Equal("Ok", result[0].Name);
    }
}
