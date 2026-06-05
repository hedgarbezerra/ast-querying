using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.ValueObjects;
using AstQuerying.Queries.Parsing.Implementations;
using AstQuerying.Queries.Tests.Support;
using AstQuerying.Queries.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class QueryValidatorTests
{
    [Fact]
    public void UnknownFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new ComparisonNode("DoesNotExist", "=", new ConstantValueNode(JsonSerializer.SerializeToElement(1)), null);

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("Unknown field", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DisallowedOperatorForFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new ComparisonNode("Id", "!=", new ConstantValueNode(JsonSerializer.SerializeToElement(1)), null);

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("not allowed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsedValidFilterPassesValidation()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var dto = new FilterClauseDto
        {
            Field = "Age",
            Op = ">",
            Value = JsonSerializer.SerializeToElement(18)
        };
        var node = QueryFilterParser.Parse(dto);

        validator.Validate(typeof(Person), node);
    }
}
