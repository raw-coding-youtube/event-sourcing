<Query Kind="Program">
  <Reference Relative="..\bin\Debug\net6.0\Npgsql.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Npgsql.dll</Reference>
  <Reference Relative="..\bin\Debug\net6.0\Snapshotter.dll">D:\WS\Rider\EventSourcing\Snapshotter\bin\Debug\net6.0\Snapshotter.dll</Reference>
  <Namespace>Npgsql</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

// https://www.npgsql.org/doc/basic-usage.html
async Task Main()
{
	await using var conn = new NpgsqlConnection(Connection.ConnectionString);
	await conn.OpenAsync();


	var userId = "c31e05e7-f966-46a7-9bb8-eb0ca3bcc95a";
	await using var cmd2 = new NpgsqlCommand($"SELECT payload FROM projections where Id = '{userId}' and type = 'Cart'", conn);
	await using var reader2 = await cmd2.ExecuteReaderAsync();

	var exists = await reader2.ReadAsync();
	exists.Dump();
}