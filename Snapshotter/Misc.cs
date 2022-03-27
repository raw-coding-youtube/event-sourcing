using System.Text.Json;
using System.Text.Json.Serialization;

public class Connection
{
    public const string ConnectionString = "host=127.0.0.1;port=5666;database=snapshotter;user id=postgres;password=password;";
    public const string NewEventChannel = "new_events";
}

public class Event
{
    public Guid Id { get; set; }
    public string Payload { get; set; }
    public string Type { get; set; }

    public static Event Create(object payload)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Payload = JsonSerializer.Serialize(payload),
            Type = payload.GetType().Name,
        };
    }
}

public class Payload
{
    public static object Parse(string type, string json) =>
        type switch
        {
            "AddedCart" => JsonSerializer.Deserialize<AddedCart>(json),
            "AddedToCart" => JsonSerializer.Deserialize<AddedToCart>(json),
            "AddedShippingInformationCart" => JsonSerializer.Deserialize<AddedShippingInformationCart>(json),
        };
}

public record AddedCart(string UserId);
public record AddedToCart(int ProductId, string ProductName, int Qty);
public record AddedShippingInformationCart(string Address, string PhoneNumber);

public class Cart
{
    public string UserId { get; set; }
    public List<Product> Products { get; set; } = new();
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Qty { get; set; }
}