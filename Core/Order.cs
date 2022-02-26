namespace Core;

public class Order
{
    public string UserId { get; set; }
    public List<Product> Products { get; set; } = new();
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
}

public class Product
{
    public int Id { get; set; }
}