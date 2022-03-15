using Microsoft.AspNetCore.Mvc.ModelBinding;
using WhenToSnapshot;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (IStore store) =>
{
    var events = await store.GetEvents("id");
    return events.Aggregate(Projection.New, Projection.Append);
});

app.MapGet("/stale", async (IStore store) =>
{
    Projection projection = await store.GetDoc("id");
    return projection;
});

app.MapGet("/live", async (IStore store) =>
{
    Projection projection = await store.GetDoc("id");
    var events = await store.GetEvents("id", projection.Version);
    if (events.Any())
    {
        return events.Aggregate(projection, Projection.Append);
    }
    return projection;
});

app.MapGet("/live-read-through", async (IStore store) =>
{
    Projection projection = await store.GetDoc("id");
    var events = await store.GetEvents("id", projection.Version);
    if (events.Any())
    {
        projection = events.Aggregate(projection, Projection.Append);
        await store.InsertDoc("id", projection);
        return projection;
    }
    return projection;
});


app.MapPost("/write-update", async (IStore store) =>
{
    Projection projection = await store.GetDoc("id");

    var newEvents = new Event[] { new(), new() };
    store.Append("id", newEvents);

    projection = newEvents.Aggregate(projection, Projection.Append);
    store.UpsertDoc("id", projection);

    await store.SaveChangesAsync();
    return "id";
});


app.MapPost("/async", async (IStore store, IQueue queue) =>
{
    var newEvents = new Event[] { new(), new() };
    store.Append("id", newEvents);

    await store.SaveChangesAsync();

    foreach (var e in newEvents)
    {
        await queue.PublishAsync(e);
    }

    return "id";
});


app.MapPost("/async-better", async (IStore store, IQueue queue) =>
{
    var newEvents = new Event[] { new(), new() };
    store.Append("id", newEvents);
    await store.SaveChangesAsync();
    return "id";
});

app.Run();