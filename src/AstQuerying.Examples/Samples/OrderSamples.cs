using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using AstQuerying.Examples.DTOs;

namespace AstQuerying.Examples.Samples;

public static class OrderSamples
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public static class Json
    {
        public static string EmptyOrder => JsonSerializer.Serialize(Objects.EmptyOrder, _jsonOptions);

        public static string OrderWithTwoItems => JsonSerializer.Serialize(Objects.OrderWithTwoItems, _jsonOptions);
    }
}

public static class Objects
{
    public static Order EmptyOrder => new Order(Guid.CreateVersion7(), 19281, []);

    public static Order OrderWithTwoItems
    {
        get
        {
            var items = new Dictionary<Guid, OrderItem>()
            {
                { Guid.CreateVersion7(), new OrderItem(19.99m, "SKU-12345", 2) },
                { Guid.CreateVersion7(), new OrderItem(29.99m, "SKU-67890", 1) }
            };
            return new Order(Guid.CreateVersion7(), 19281, items);
        }
    }

    public static Order ExpensiveOrder
    {
        get
        {
            var items = new Dictionary<Guid, OrderItem>()
            {
                { Guid.CreateVersion7(), new OrderItem(19.99m, "SKU-12345", 2) },
                { Guid.CreateVersion7(), new OrderItem(149.99m, "SKU23445", 62) },
                { Guid.CreateVersion7(), new OrderItem(569.99m, "SKU-351", 75) },
                { Guid.CreateVersion7(), new OrderItem(24239.99m, "SKU-67890", 1) }
            };
            return new Order(Guid.CreateVersion7(), 19, items);
        }
    }
}