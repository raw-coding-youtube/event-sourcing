<Query Kind="Program">
  <Reference Relative="Core\bin\Debug\net6.0\Core.dll">D:\WS\Rider\EventSourcing\Core\bin\Debug\net6.0\Core.dll</Reference>
  <Namespace>Core</Namespace>
</Query>

void Main()
{
	var events = new List<Event> {
		new AddedCart("userId"),
		new AddedItemToCart(1, 2),
		new AddedItemToCart(7, 1),
		new AddedItemToCart(9, 2),
		new AddedShippingInformationCart("42 Road Lane", "12343218764"),
	};

	var orderAggregate = (Order order, Event e) =>
	{
		if (e is AddedCart ac)
		{
			order.UserId = ac.UserId;
		}
		else if (e is AddedItemToCart ai)
		{
			for (int i = 0; i < ai.Qty; i++)
			{
				order.Products.Add(new Product() { Id = ai.ProductId });
			}
		}
		else if (e is RemovedItemFromCart ri)
		{
			for (int i = 0; i < ri.Qty; i++)
			{
				var productToRemove = order.Products.FirstOrDefault(x => x.Id == ri.ProductId);
				order.Products.Remove(productToRemove);
			}
		}
		else if (e is AddedShippingInformationCart asi)
		{
			order.Address = asi.Address;
			order.PhoneNumber = asi.PhoneNumber;
		}
		return order;
	};

	var order1 = events.Aggregate(new Order(), orderAggregate);
	
	order1.Dump();


	var events2 = new List<Event> {
		new RemovedItemFromCart(1, 1),
	};

	var order2 = events2.Aggregate(order1, orderAggregate);

	order2.Dump();
}