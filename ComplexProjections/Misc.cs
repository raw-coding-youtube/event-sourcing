namespace ComplexProjections;

# region BASE

public record Event;

public interface IStore
{
    T GetAsync<T>(string id) where T : class;
    T ListAsync<T>(string[] ids) where T : class;
    List<T> AllAsync<T>() where T : class;
    Task Update<T>(T v) where T : class;
}

# endregion

#region Hard Read

public record Posted : Event;

public record UpdatedAvatar : Event;

public record Post(string Id, string Content, string UserId);

public record User(string Id, string Username, string Avatar);

#endregion

#region Hard Write

public record Created : Event;

public record Viewed : Event;

public record Commented : Event;

public record Preview(string Id, string Title, int Views);

public record Full(string Id, string Title, string Description);

public record AdminPreview(string Id, string Title, string Description, int Status, int Comments);

#endregion