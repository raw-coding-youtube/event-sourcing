<Query Kind="Program">
  <Reference Relative="..\bin\Debug\net6.0\Npgsql.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Npgsql.dll</Reference>
  <Reference Relative="..\bin\Debug\net6.0\Snapshotter.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Snapshotter.dll</Reference>
  <Namespace>Npgsql</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

// https://www.postgresql.org/docs/current/sql-notify.html
// https://www.npgsql.org/doc/basic-usage.html
async Task Main()
{
	var userId = "c31e05e7-f966-46a7-9bb8-eb0ca3bcc95a";
	//var e = Event.Create(new AddedCart(userId));
	//var e = Event.Create(new AddedShippingInformationCart("Road Street", "999"));
	var e = Event.Create(new AddedToCart(2, "Table 3000", 1));

	await using var conn = new NpgsqlConnection(Connection.ConnectionString);
	await conn.OpenAsync();

	var q = $"INSERT INTO events VALUES ('{e.Id}','{e.Payload}','{e.Type}')";
	await using var batch = new NpgsqlBatch(conn)
	{
		BatchCommands =
		{
			new(q),
			new($"NOTIFY {Connection.NewEventChannel}, '{e.Id}'")
		}
	};

	await using var reader = await batch.ExecuteReaderAsync();
}