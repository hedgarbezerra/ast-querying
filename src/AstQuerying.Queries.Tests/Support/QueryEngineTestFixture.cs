using System.Reflection;
using AstQuerying.Queries.Configuration.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Tests.Support;

public static class QueryEngineTestFixture
{
    public static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddQueryEngine(Assembly.GetExecutingAssembly());
        return services.BuildServiceProvider();
    }
}
