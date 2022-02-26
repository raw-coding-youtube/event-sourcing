namespace Core;

public record Event
{
    public string Name => GetType().Name;
}


public record AddedCart(string UserId) : Event;
public record AddedItemToCart(int ProductId, int Qty) : Event;
public record RemovedItemFromCart(int ProductId, int Qty) : Event;
public record AddedShippingInformationCart(string Address, string PhoneNumber) : Event;