using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configuration.DependencyInjection;
using AstQuerying.Queries.Metadata.Contracts;
using AstQuerying.Queries.Sorting.Contracts;
using AstQuerying.Queries.Sorting.Implementations;
using AstQuerying.Queries.Sorting.ValueObjects;
using AstQuerying.Queries.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class MetadataSortAndPaginationTests
{
    [Fact]
    public void GenericMetadataProviderReturnsConfiguredPropertyPaths()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var provider = sp.GetRequiredService<IQueryMetadataProvider<Person>>();
        var meta = provider.GetMetadataForEntity();
        var paths = meta.Properties.Select(p => p.Path).OrderBy(x => x).ToArray();

        Assert.Equal(new[] { "Age", "Id", "Name" }, paths);
    }

    [Fact]
    public void SortBuilderBuildsNameKeySelectorWithAscendingDirection()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var sortBuilder = sp.GetRequiredService<ISortBuilder<Person>>();
        var lambda = sortBuilder.BuildKeySelector(new SortFieldDto { Field = "Name", Direction = "asc" });

        Assert.Equal(typeof(string), lambda.ReturnType);
        var compiled = (Func<Person, string>)lambda.Compile();
        var person = new Person { Id = 1, Name = "Test", Age = 1 };
        Assert.Equal("Test", compiled(person));
    }

    [Fact]
    public void SortBuilderRejectsEmptySortList()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var sortBuilder = sp.GetRequiredService<ISortBuilder<Person>>();
        var source = new[] { new Person { Id = 1, Name = "A", Age = 1 } }.AsQueryable();

        Assert.Throws<QueryValidationException>(() => sortBuilder.Apply(source, Array.Empty<SortFieldDto>()));
    }

    [Fact]
    public void SortBuilderRejectsUnknownField()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var sortBuilder = sp.GetRequiredService<ISortBuilder<Person>>();
        var source = Array.Empty<Person>().AsQueryable();

        Assert.Throws<QueryValidationException>(() =>
            sortBuilder.Apply(source, new List<SortFieldDto> { new() { Field = "Missing", Direction = "asc" } }));
    }

    [Fact]
    public void PaginationResolverEnforcesMaxTake()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var options = sp.GetRequiredService<QueryEngineOptions>();

        var ex = Assert.Throws<QueryValidationException>(() =>
            PaginationResolver.Resolve(new PaginationRequestDto { Take = options.MaxTake + 1 }, options));

        Assert.Contains("take", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaginationResolverRejectsMixedPagingModes()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var options = sp.GetRequiredService<QueryEngineOptions>();

        Assert.Throws<QueryValidationException>(() =>
            PaginationResolver.Resolve(
                new PaginationRequestDto { Page = 1, Skip = 0 },
                options));
    }

    [Fact]
    public void PaginationResolverComputesSkipFromPage()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var options = sp.GetRequiredService<QueryEngineOptions>();

        var (skip, take) = PaginationResolver.Resolve(
            new PaginationRequestDto { Page = 3, PageSize = 10 },
            options);

        Assert.Equal(20, skip);
        Assert.Equal(10, take);
    }
}
