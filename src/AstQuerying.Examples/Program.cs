using AstQuerying.Examples.Samples;
using AstQuerying.Queries.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQueryEngine(options =>
{
    options.DefaultPageSize = 1;
    options.MaxPageSize = 3;
});

using var host = builder.Build();
PersonSamples.RunAll(host.Services);
