namespace WhenToSnapshot;

public class Event
{
    public string Id { get; set; }
    public int Version { get; set; }
}

public class Projection
{
    public static Projection New => new();
    public int Version { get; set; }

    public static Projection Append(Projection seed, Event @event)
    {
        return null;
    }
}

public interface IStore
{
    Task<Projection> GetDoc(string id);
    Task<Projection[]> GetDocs(string type);
    Task InsertDoc(string id, Projection data);

    Task<Event[]> GetEvents(string id);
    Task<Event[]> GetEvents(string id, int fromVersion);

    void UpsertDoc(string id, Projection data);
    void Append(string aggregateId, params Event[] e);
    Task SaveChangesAsync();
}

public interface IQueue
{
    Task PublishAsync(Event e);
}