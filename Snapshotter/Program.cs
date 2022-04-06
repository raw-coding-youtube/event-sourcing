using System.Text.Json;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Projector>();
var app = builder.Build();

app.MapGet("/", async () =>
{
    await using var conn = new NpgsqlConnection(Connection.ConnectionString);
    await conn.OpenAsync();

    var userId = "c31e05e7-f966-46a7-9bb8-eb0ca3bcc95a";
    await using var cmd2 = new NpgsqlCommand($"SELECT payload FROM projections where Id = '{userId}' and type = 'Cart'", conn);
    await using var reader2 = await cmd2.ExecuteReaderAsync();

    var exists = await reader2.ReadAsync();
    return exists ? reader2.GetString(0) : "";
});

app.MapPost("/{type}", async (string type) =>
{
    var userId = "c31e05e7-f966-46a7-9bb8-eb0ca3bcc95a";
    var e = type switch
    {
        "AddedCart" => Event.Create(new AddedCart(userId)),
        "AddedShippingInformationCart" => Event.Create(new AddedShippingInformationCart("Road Street", "999")),
        "Table" => Event.Create(new AddedToCart(2, "Table 3000", 1)),
        _ => Event.Create(new AddedToCart(1, "Anything", 3)),
    };

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
});

app.Run();

public class Projector : BackgroundService
{
    private readonly ILogger<Projector> _logger;

    public Projector(ILogger<Projector> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("projector started!!!");
        await using var conn = new NpgsqlConnection(Connection.ConnectionString);
        await conn.OpenAsync();
        while (true)
        {
            await using var cmd2 = new NpgsqlCommand($"SELECT pg_try_advisory_lock(1)", conn);
            await using var reader2 = await cmd2.ExecuteReaderAsync();

            await reader2.ReadAsync();
            var gotLock = reader2.GetBoolean(0);
            if (gotLock)
            {
                _logger.LogInformation("Lock Acquired!!!");
                break;
            }

            _logger.LogInformation("Lock Busy...");
            await Task.Delay(5000);
        }

        conn.Notification += async (o, e) =>
        {
            _logger.LogInformation($"processing {e.Payload}");
            await HandleEvent(new Guid(e.Payload));
        };

        await using (var cmd = new NpgsqlCommand($"LISTEN {Connection.NewEventChannel}", conn))
        {
            cmd.ExecuteNonQuery();
        }

        while (true)
        {
            await conn.WaitAsync();
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
}