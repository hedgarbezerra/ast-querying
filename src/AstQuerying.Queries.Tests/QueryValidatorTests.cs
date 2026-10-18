using System.Text.Json;
using AstQuerying.Queries.Ast.Parsing;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Tests.Support;
using AstQuerying.Queries.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class QueryValidatorTests
{
    [Fact]
    public void UnknownFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new SingleValueComparisonNode("DoesNotExist", "=",
            new ConstantValueNode(JsonSerializer.SerializeToElement(1)));

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("Unknown field", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DisallowedOperatorForFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new SingleValueComparisonNode("Id", "!=",
            new ConstantValueNode(JsonSerializer.SerializeToElement(1)));

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("not allowed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsedValidFilterPassesValidation()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var parser = sp.GetRequiredService<IQueryFilterParser>();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var dto = new FilterClauseDto
        {
            Field = "Age",
            Operator = ">",
            Value = JsonSerializer.SerializeToElement(18)
        };
        var node = parser.Parse(dto);

        validator.Validate(typeof(Person), node);
    }

    [Fact]
    public void TemporalFunctionOnIntegralFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new SingleValueComparisonNode("Age", ">=", new FunctionValueNode("Today"));

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("Today", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cannot be applied", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IntegralPolicyFunctionOnTemporalFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new SingleValueComparisonNode("BirthDate", ">=", new FunctionValueNode("LegalAge"));

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("LegalAge", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cannot be applied", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DecimalPolicyFunctionOnTemporalFieldThrowsQueryValidationException()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var node = new SingleValueComparisonNode("BirthDate", ">=", new FunctionValueNode("MinimumWage"));

        var ex = Assert.Throws<QueryValidationException>(() => validator.Validate(typeof(Person), node));
        Assert.Contains("MinimumWage", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cannot be applied", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}