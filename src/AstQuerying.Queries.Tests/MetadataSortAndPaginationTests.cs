using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configurations;
using AstQuerying.Queries.Metadata;
using AstQuerying.Queries.Sorting;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class MetadataSortAndPaginationTests
{
    [Fact]
    public void GenericMetadataProviderReturnsConfiguredPropertyPaths()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var provider = sp.GetRequiredService<IQueryMetadataProvider>();
        var meta = provider.GetMetadataForEntity<Person>();
        var paths = meta.Properties.Select(p => p.Path).OrderBy(x => x).ToArray();

        Assert.Equal(new[] { "Age", "BirthDate", "Id", "Name", "Salary" }, paths);
    }

    [Fact]
    public void SortingKeyBuilderBuildsNameKeySelectorWithAscendingDirection()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var keyBuilder = sp.GetRequiredService<ISortingKeyBuilder>();
        var lambda = keyBuilder.Build<Person>(new SortFieldDto { Field = "Name", Direction = SortDirection.Ascending });

        Assert.Equal(typeof(string), lambda.ReturnType);
        var compiled = (Func<Person, string>)lambda.Compile();
        var person = new Person { Id = 1, Name = "Test", Age = 1 };
        Assert.Equal("Test", compiled(person));
    }

    [Fact]
    public void SortingServiceRejectsEmptySortList()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var sortingService = sp.GetRequiredService<ISortingService>();
        var source = new[] { new Person { Id = 1, Name = "A", Age = 1 } }.AsQueryable();

        Assert.Throws<QueryValidationException>(() => sortingService.Apply(source, Array.Empty<SortFieldDto>()));
    }

    [Fact]
    public void SortingServiceRejectsUnknownField()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var sortingService = sp.GetRequiredService<ISortingService>();
        var source = Array.Empty<Person>().AsQueryable();

        Assert.Throws<QueryValidationException>(() =>
            sortingService.Apply(source,
                new List<SortFieldDto> { new() { Field = "Missing", Direction = SortDirection.Ascending } }));
    }

    [Fact]
    public void PaginationResolverEnforcesMaxPageSize()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var options = sp.GetRequiredService<QueryEngineOptions>();
        var resolver = sp.GetRequiredService<IPaginationResolver>();

        var ex = Assert.Throws<QueryValidationException>(() =>
            resolver.Resolve(new PaginationDto { Page = 1, Size = options.MaxPageSize + 1 }));

        Assert.Contains("size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PaginationResolverUsesDefaultsWhenDtoNull()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var options = sp.GetRequiredService<QueryEngineOptions>();
        var resolver = sp.GetRequiredService<IPaginationResolver>();

        var (skip, take) = resolver.Resolve(null);

        Assert.Equal(0, skip);
        Assert.Equal(options.DefaultPageSize, take);
    }

    [Fact]
    public void PaginationResolverComputesSkipFromOneBasedPage()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var resolver = sp.GetRequiredService<IPaginationResolver>();

        var (skip, take) = resolver.Resolve(
            new PaginationDto { Page = 3, Size = 10 });

        Assert.Equal(20, skip);
        Assert.Equal(10, take);
    }
}