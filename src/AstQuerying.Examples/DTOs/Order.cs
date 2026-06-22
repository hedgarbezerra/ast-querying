using System.Collections.ObjectModel;

namespace AstQuerying.Examples.DTOs;

public record OrderItem(decimal Price, string Sku, int Quantity);

public record Order(Guid Id, long Number, Dictionary<Guid, OrderItem> OrderItems)
{
    public decimal Total => OrderItems.Values.Sum(o => o.Price);
    public int Quantity => OrderItems.Values.Sum(o => o.Quantity);
    public int DistinctCount => OrderItems.Keys.Count;
}