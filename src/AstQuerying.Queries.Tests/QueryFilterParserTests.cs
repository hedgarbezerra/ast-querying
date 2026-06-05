using System.Text.Json;
using AstQuerying.Queries.Ast.ValueObjects;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.ValueObjects;
using AstQuerying.Queries.Parsing.Implementations;

namespace AstQuerying.Queries.Tests;

public sealed class QueryFilterParserTests
{
    [Fact]
    public void ParsesAndTreeWithTwoComparisons()
    {
        var root = new FilterClauseDto
        {
            And =
            [
                new FilterClauseDto
                {
                    Field = "Age",
                    Op = "=",
                    Value = JsonSerializer.SerializeToElement(21)
                },
                new FilterClauseDto
                {
                    Field = "Name",
                    Operator = "contains",
                    Value = JsonSerializer.SerializeToElement("a")
                }
            ]
        };

        var node = QueryFilterParser.Parse(root);
        var and = Assert.IsType<AndNode>(node);
        Assert.Equal(2, and.Children.Count);
        var c0 = Assert.IsType<ComparisonNode>(and.Children[0]);
        Assert.Equal("Age", c0.FieldPath);
        Assert.Equal("=", c0.OperatorName);
        Assert.IsType<ConstantValueNode>(c0.Value);
        var c1 = Assert.IsType<ComparisonNode>(and.Children[1]);
        Assert.Equal("Name", c1.FieldPath);
        Assert.Equal("contains", c1.OperatorName);
    }

    [Fact]
    public void ParsesNotWithNestedComparison()
    {
        var root = new FilterClauseDto
        {
            Not = new FilterClauseDto
            {
                Field = "Id",
                Op = "=",
                Value = JsonSerializer.SerializeToElement(1)
            }
        };

        var node = QueryFilterParser.Parse(root);
        var not = Assert.IsType<NotNode>(node);
        var inner = Assert.IsType<ComparisonNode>(not.Child);
        Assert.Equal("Id", inner.FieldPath);
    }

    [Fact]
    public void ParsesInOperatorWithMultipleValues()
    {
        var root = new FilterClauseDto
        {
            Field = "Id",
            Operator = "in",
            Values =
            [
                JsonSerializer.SerializeToElement(1),
                JsonSerializer.SerializeToElement(2)
            ]
        };

        var node = QueryFilterParser.Parse(root);
        var cmp = Assert.IsType<ComparisonNode>(node);
        Assert.Equal("in", cmp.OperatorName);
        Assert.NotNull(cmp.Values);
        Assert.Equal(2, cmp.Values.Count);
    }

    [Fact]
    public void MissingOperatorThrowsQueryValidationException()
    {
        var root = new FilterClauseDto { Field = "Name", Value = JsonSerializer.SerializeToElement("x") };

        var ex = Assert.Throws<QueryValidationException>(() => QueryFilterParser.Parse(root));
        Assert.Contains("operator", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmptyClauseThrowsQueryValidationException()
    {
        var root = new FilterClauseDto();

        Assert.Throws<QueryValidationException>(() => QueryFilterParser.Parse(root));
    }
}
