using ComplexProjections;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

#region Hard Read

app.MapGet("/posts", (IStore store) =>
{
    var posts = store.AllAsync<Post>();
    var users = store.ListAsync<User>(posts.Select(x => x.UserId).ToArray());
    return new { posts, users };
});

#endregion

#region Hard Write

app.MapPost("/comment", async (string videoId, string comment, IStore store) =>
{
    var e = new Commented();
    // projections to update when event ...


    var adminPreview = store.GetAsync<AdminPreview>(videoId);
    adminPreview = adminPreview with { Comments = adminPreview.Comments + 1 };
    await store.Update(adminPreview);
});

#endregion

app.Run();