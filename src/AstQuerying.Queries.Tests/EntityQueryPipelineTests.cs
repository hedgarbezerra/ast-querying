using System.Text.Json;
using AstQuerying.Queries.Common.Pipeline.Contracts;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class EntityQueryPipelineTests
{
    [Fact]
    public void ResolvesFromDi()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var pipeline = sp.GetRequiredService<IEntityQueryPipeline>();
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void ApplyUsesExplicitSortFilterAndPagination()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var pipeline = sp.GetRequiredService<IEntityQueryPipeline>();
        var source = new[]
        {
            new Person { Id = 3, Name = "Carla", Age = 25 },
            new Person { Id = 1, Name = "Alice", Age = 20 },
            new Person { Id = 2, Name = "Bruno", Age = 30 }
        }.AsQueryable();

        var filter = new FilterClauseDto
        {
            Field = "Age",
            Operator = ">=",
            Value = JsonSerializer.SerializeToElement(21)
        };
        var sort = new List<SortFieldDto> { new() { Field = "Name", Direction = SortDirection.Ascending } };
        var page = new PaginationDto { Page = 1, Size = 10 };

        var list = pipeline.Apply(source, filter, sort, page).ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("Bruno", list[0].Name);
        Assert.Equal("Carla", list[1].Name);
    }

    [Fact]
    public void ApplyUsesDefaultSortWhenSortIsNull()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var pipeline = sp.GetRequiredService<IEntityQueryPipeline>();
        var source = new[]
        {
            new Person { Id = 3, Name = "C", Age = 1 },
            new Person { Id = 1, Name = "A", Age = 1 }
        }.AsQueryable();

        var list = pipeline.Apply(source, null, null, new PaginationDto { Page = 1, Size = 10 }).ToList();

        Assert.Equal(new[] { 1, 3 }, list.Select(p => p.Id));
    }
}
