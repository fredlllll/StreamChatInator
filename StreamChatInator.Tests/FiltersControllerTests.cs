using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using StreamChatInator.Controllers;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;

namespace StreamChatInator.Tests;

public class FiltersControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _db;

    public FiltersControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private FiltersController NewController()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        //TODO: constructor changed, let AI fix this
        //var history = new EventHistoryService(_db, NullLogger<EventHistoryService>.Instance, cache);
        return new FiltersController(_db, null!);
    }

    private ChatEventFilter CreateFilter(string codeJs = "function __matches(eventData) { return true; }")
    {
        var filter = new ChatEventFilter
        {
            Id = Model.GetNewId<ChatEventFilter>(),
            Name = "test",
            Code = "// compiled",
            CodeJs = codeJs,
        };
        _db.ChatEventFilters.Add(filter);
        _db.SaveChanges();
        return filter;
    }

    private void SeedUserJoined(string username, DateTime created)
    {
        var data = new ChatEventUserJoined
        {
            Id = Model.GetNewId<ChatEventUserJoined>(),
            Username = username,
            Channel = "testchannel",
            Created = created,
            Updated = created,
        };
        var chatEvent = new ChatEvent
        {
            Id = Model.GetNewId<ChatEvent>(),
            ChatEventType = ChatEventType.UserJoined,
            EventId = data.Id,
            Created = created,
            Updated = created,
        };
        _db.Add(data);
        _db.Add(chatEvent);
    }

    private List<string> PageAll(string filterId, int take)
    {
        var controller = NewController();
        var allIds = new List<string>();
        string? cursor = null;
        for (int page = 0; page < 50; page++)
        {
            var response = controller.GetMessages(filterId, cursor, take).Value
                ?? throw new InvalidOperationException("expected a successful result");

            allIds.AddRange(response.Events.Select(e => e.EventId));

            if (!response.HasMore)
            {
                return allIds;
            }
            Assert.False(string.IsNullOrEmpty(response.NextCursor));
            cursor = response.NextCursor;
        }
        throw new InvalidOperationException("paging did not terminate");
    }

    [Fact]
    public void GetMessages_ReturnsEventsNewestFirst()
    {
        var filter = CreateFilter();
        var now = DateTime.UtcNow;
        SeedUserJoined("oldest", now.AddMinutes(-3));
        SeedUserJoined("middle", now.AddMinutes(-2));
        SeedUserJoined("newest", now.AddMinutes(-1));
        _db.SaveChanges();

        var response = NewController().GetMessages(filter.Id, null, 50).Value!;
        var usernames = response.Events.Select(e => ((ChatEventUserJoined)e.ChatEventData).Username).ToArray();

        Assert.Equal(new[] { "newest", "middle", "oldest" }, usernames);
        Assert.False(response.HasMore);
    }

    [Fact]
    public void GetMessages_PagesAcrossBatchBoundary_WithoutSkipsOrDuplicates()
    {
        var filter = CreateFilter();
        var t = DateTime.UtcNow.AddMinutes(-1);
        const int total = 250; // > batchSize (200)
        for (int i = 0; i < total; i++)
        {
            SeedUserJoined($"user{i:D3}", t);
        }
        _db.SaveChanges();

        var allIds = PageAll(filter.Id, take: 50);

        Assert.Equal(total, allIds.Count);
        Assert.Equal(total, allIds.Distinct().Count());
    }

    [Fact]
    public void GetMessages_BareTimestampCursor_ReturnsEventsStrictlyBeforeIt()
    {
        var filter = CreateFilter();
        var boundary = DateTime.UtcNow.AddMinutes(-2);
        SeedUserJoined("before1", boundary.AddMinutes(-5));
        SeedUserJoined("before2", boundary.AddMinutes(-4));
        SeedUserJoined("after", boundary.AddMinutes(1));
        _db.SaveChanges();

        var response = NewController().GetMessages(filter.Id, boundary.ToString("o"), 50).Value!;
        var usernames = response.Events.Select(e => ((ChatEventUserJoined)e.ChatEventData).Username).ToArray();

        Assert.Equal(new[] { "before2", "before1" }, usernames);
    }

    [Fact]
    public void GetMessages_AppliesJsFilter()
    {
        var filter = CreateFilter(
            "function __matches(eventData) { return eventData.chatEventData.username === 'alice'; }");
        var t = DateTime.UtcNow;
        SeedUserJoined("alice", t);
        SeedUserJoined("bob", t);
        SeedUserJoined("alice", t.AddSeconds(1));
        _db.SaveChanges();

        var response = NewController().GetMessages(filter.Id, null, 50).Value!;
        var usernames = response.Events.Select(e => ((ChatEventUserJoined)e.ChatEventData).Username).ToArray();

        Assert.Equal(2, usernames.Length);
        Assert.All(usernames, u => Assert.Equal("alice", u));
    }

    [Fact]
    public void GetMessages_SkipsEventsWithMissingData()
    {
        var filter = CreateFilter();
        var t = DateTime.UtcNow;
        SeedUserJoined("good", t);
        _db.Add(new ChatEvent
        {
            Id = Model.GetNewId<ChatEvent>(),
            ChatEventType = ChatEventType.UserJoined,
            EventId = "does-not-exist",
            Created = t,
            Updated = t,
        });
        _db.SaveChanges();

        var response = NewController().GetMessages(filter.Id, null, 50).Value!;
        var data = Assert.Single(response.Events);

        Assert.Equal("good", ((ChatEventUserJoined)data.ChatEventData).Username);
    }

    [Fact]
    public void GetMessages_UnknownFilter_ReturnsNotFound()
    {
        var result = NewController().GetMessages("missing", null, 50);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
