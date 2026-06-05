using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Metadata.Contracts;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Tests.Support;
using AstQuerying.Queries.Validation.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests;

public sealed class AddQueryEngineTests
{
    [Fact]
    public void ResolvesRegistryWithPersonEntityAndBuiltInOperators()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var registry = sp.GetRequiredService<IQueryRegistry>();

        Assert.NotNull(registry);
        Assert.True(registry.Entities.ContainsKey(typeof(Person)));

        var expectedOps = new[]
        {
            "=", "!=", ">", ">=", "<", "<=", "contains", "startsWith", "endsWith", "in", "notIn", "isNull", "isNotNull"
        };

        foreach (var key in expectedOps)
            Assert.True(registry.Operators.ContainsKey(key), $"Missing operator: {key}");

        var expectedFns = new[] { "Today", "Now", "StartOfMonth", "EndOfMonth", "CurrentYear" };
        foreach (var key in expectedFns)
            Assert.True(registry.Functions.ContainsKey(key), $"Missing function: {key}");
    }

    [Fact]
    public void ResolvesValidatorAndEnumerableFilterOperatorsFromDi()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var validator = sp.GetRequiredService<IQueryValidator>();
        var operators = sp.GetServices<IFilterOperator>().Select(o => o.Name).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.NotNull(validator);
        Assert.Contains("=", operators);
        Assert.Contains("in", operators);
    }

    [Fact]
    public void ResolvesEnumerableFunctionProvidersFromDi()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var names = sp.GetServices<IFunctionProvider>().Select(f => f.Name).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Today", names);
        Assert.Contains("CurrentYear", names);
    }

    [Fact]
    public void ResolvesMetadataProvider()
    {
        using var sp = QueryEngineTestFixture.CreateProvider();
        var provider = sp.GetRequiredService<IQueryMetadataProvider>();

        var meta = provider.GetMetadata(typeof(Person));
        Assert.NotNull(meta);
        Assert.Equal(typeof(Person), meta.ClrType);
    }
}
