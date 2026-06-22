using AstQuerying.Queries.Configuration.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQueryEngine(typeof(Program).Assembly);