<Query Kind="Program">
  <Reference Relative="..\bin\Debug\net6.0\Npgsql.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Npgsql.dll</Reference>
  <Reference Relative="..\bin\Debug\net6.0\Snapshotter.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Snapshotter.dll</Reference>
  <Namespace>Npgsql</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

// https://www.npgsql.org/doc/wait.html
// https://www.postgresql.org/docs/9.1/sql-listen.html
async Task Main()
{
	await using var conn = new NpgsqlConnection(Connection.ConnectionString);
	await conn.OpenAsync();
	conn.Notification += async (o, e) =>
	{
		await HandleEvent(new Guid(e.Payload));
	};

	await using (var cmd = new NpgsqlCommand($"LISTEN {Connection.NewEventChannel}", conn))
	{
		cmd.ExecuteNonQuery();
	}

	while (true)
	{
		await conn.WaitAsync();
		"Recieved Event!".Dump("Recieved Event!");
	}
}

public async Task HandleEvent(Guid eventId)
{
	await using var conn = new NpgsqlConnection(Connection.ConnectionString);
	await conn.OpenAsync();

	await using var cmd = new NpgsqlCommand($"SELECT * FROM events where Id = '{eventId}'", conn);
	await using var reader = await cmd.ExecuteReaderAsync();

	await reader.ReadAsync();
	var payload = reader.GetString(1);
	var type = reader.GetString(2);

	var obj = Payload.Parse(type, payload);
	obj.Dump();
	
	await reader.DisposeAsync();
	await cmd.DisposeAsync();

	var userId = "c31e05e7-f966-46a7-9bb8-eb0ca3bcc95a";
	await using var cmd2 = new NpgsqlCommand($"SELECT payload FROM projections where Id = '{userId}' and type = 'Cart'", conn);
	await using var reader2 = await cmd2.ExecuteReaderAsync();

	var exists = await reader2.ReadAsync();
	var cart = exists ? JsonSerializer.Deserialize<Cart>(reader2.GetString(0)) : new Cart();

	await reader2.DisposeAsync();
	await cmd2.DisposeAsync();
	
	if (obj is AddedCart ac)
	{
		cart.UserId = ac.UserId;
	}
	else if (obj is AddedToCart atc)
	{
		cart.Products.Add(new Product
		{
			Id = atc.ProductId,
			Name = atc.ProductName,
			Qty = atc.Qty
		});
	}
	else if (obj is AddedShippingInformationCart si)
	{
		cart.Address = si.Address;
		cart.PhoneNumber = si.PhoneNumber;
	}

	var newPayload = JsonSerializer.Serialize<object>(cart);
	var upsertquery = $@"
insert into projections values('{userId}', '{newPayload}', 'Cart')
on conflict on constraint type_id
do update set payload = '{newPayload}';
";
	await using var cmd3 = new NpgsqlCommand(upsertquery, conn);
	await cmd3.ExecuteNonQueryAsync();
	await cmd3.DisposeAsync();
}












