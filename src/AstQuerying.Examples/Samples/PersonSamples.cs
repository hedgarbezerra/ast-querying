using System.Text.Json;
using System.Text.Json.Serialization;
using AstQuerying.Examples.DTOs;
using AstQuerying.Queries.Ast.Parsing;
using AstQuerying.Queries.Common.Pipeline;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Examples.Samples;

/// <summary>
///     Demonstrates end-to-end query flows against in-memory <see cref="Person" /> data.
/// </summary>
public static class PersonSamples
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    ///     Runs both demonstration flows and prints results to the console.
    /// </summary>
    /// <param name="services">The configured service provider.</param>
    public static void RunAll(IServiceProvider services)
    {
        var people = CreateSampleData().AsQueryable();

        Console.WriteLine("=== Flow 1: HTTP-like JSON + Pipeline ===");
        RunPipelineDemo(services, people);
        Console.WriteLine();

        Console.WriteLine("=== Flow 2: Direct QueryNode API ===");
        RunDirectQueryNodeDemo(services, people);
    }

    private static void RunPipelineDemo(IServiceProvider services, IQueryable<Person> source)
    {
        const string filterJson = """
            {
              "and": [
                { "field": "IsActive", "operator": "=", "value": true },
                { "field": "Age", "operator": ">=", "value": 30 }
              ]
            }
            """;

        const string sortJson = """
            [
              { "field": "LastName", "direction": "asc" },
              { "field": "FirstName", "direction": "asc" }
            ]
            """;

        const string paginationJson = """{ "page": 1, "size": 10 }""";

        var filter = JsonSerializer.Deserialize<FilterClauseDto>(filterJson, JsonOptions);
        var sort = DeserializeSortFields(sortJson);
        var pagination = JsonSerializer.Deserialize<PaginationDto>(paginationJson, JsonOptions);

        var pipeline = services.GetRequiredService<IEntityQueryPipeline>();
        var result = pipeline.Apply(source, filter, sort, pagination).ToList();

        Console.WriteLine($"Filter JSON: {filterJson.Trim()}");
        Console.WriteLine($"Sort JSON: {sortJson.Trim()}");
        Console.WriteLine($"Pagination JSON: {paginationJson}");
        Console.WriteLine($"Results ({result.Count}):");
        foreach (var person in result)
            Console.WriteLine($"  - {person.FirstName} {person.LastName}, age {person.Age}");
    }

    private static void RunDirectQueryNodeDemo(IServiceProvider services, IQueryable<Person> source)
    {
        var filterDto = new FilterClauseDto
        {
            Field = "Email",
            Operator = "contains",
            Value = JsonSerializer.SerializeToElement("@example.com")
        };

        var parser = services.GetRequiredService<IQueryFilterParser>();
        var validator = services.GetRequiredService<IQueryValidator>();
        var filtering = services.GetRequiredService<IFilteringService>();

        var root = parser.Parse(filterDto);
        validator.Validate(typeof(Person), root);
        var filtered = filtering.Apply(source, root).ToList();

        Console.WriteLine("Parsed FilterClauseDto -> QueryNode -> validate -> IFilteringService.Apply");
        Console.WriteLine($"Operator: contains on Email");
        Console.WriteLine($"Results ({filtered.Count}):");
        foreach (var person in filtered)
            Console.WriteLine($"  - {person.FirstName} {person.LastName} <{person.Email}>");
    }

    private static List<SortFieldDto> DeserializeSortFields(string sortJson)
    {
        var result = new List<SortFieldDto>();
        using var document = JsonDocument.Parse(sortJson);
        foreach (var item in document.RootElement.EnumerateArray())
        {
            var field = item.GetProperty("field").GetString() ?? "";
            var directionToken = item.GetProperty("direction").GetString();
            var direction = directionToken switch
            {
                "asc" => SortDirection.Ascending,
                "desc" => SortDirection.Descending,
                _ => Enum.Parse<SortDirection>(directionToken ?? "Ascending", true)
            };
            result.Add(new SortFieldDto { Field = field, Direction = direction });
        }

        return result;
    }

    private static List<Person> CreateSampleData()
    {
        return
        [
            new Person
            {
                Id = Guid.NewGuid(), FirstName = "Ana", LastName = "Silva", Age = 28, Email = "ana@example.com",
                IsActive = true
            },
            new Person
            {
                Id = Guid.NewGuid(), FirstName = "Bruno", LastName = "Costa", Age = 35, Email = "bruno@example.com",
                IsActive = true
            },
            new Person
            {
                Id = Guid.NewGuid(), FirstName = "Carla", LastName = "Mendes", Age = 42, Email = "carla@example.com",
                IsActive = true
            },
            new Person
            {
                Id = Guid.NewGuid(), FirstName = "Diego", LastName = "Alves", Age = 31, Email = "diego@other.com",
                IsActive = true
            },
            new Person
            {
                Id = Guid.NewGuid(), FirstName = "Elena", LastName = "Rocha", Age = 29, Email = "elena@example.com",
                IsActive = false
            }
        ];
    }
}
