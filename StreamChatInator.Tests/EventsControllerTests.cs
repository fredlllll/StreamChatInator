using Microsoft.AspNetCore.SignalR;
using StreamChatInator.Controllers;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StreamChatInator.Tests;

/// <summary>
/// Minimal IHubContext/Clients/ClientProxy fakes so tests can exercise
/// controllers that broadcast over SignalR without spinning up a real hub.
/// </summary>
internal class FakeClientProxy : IClientProxy
{
    public int SendCount { get; private set; }

    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        SendCount++;
        return Task.CompletedTask;
    }
}

internal class FakeClients : IHubClients
{
    public FakeClientProxy AllProxy { get; } = new();

    public IClientProxy All => AllProxy;
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => AllProxy;
    public IClientProxy Client(string connectionId) => AllProxy;
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => AllProxy;
    public IClientProxy Group(string groupName) => AllProxy;
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => AllProxy;
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => AllProxy;
    public IClientProxy User(string userId) => AllProxy;
    public IClientProxy Users(IReadOnlyList<string> userIds) => AllProxy;
}

internal class FakeHubContext : IHubContext<StreamChatInator.Hubs.ChatHub>
{
    public FakeClients Clients { get; } = new();
    IHubClients IHubContext<StreamChatInator.Hubs.ChatHub>.Clients => Clients;
    public IGroupManager Groups => throw new NotImplementedException();
}

public class EventsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IServiceScope _scope;
    private readonly DatabaseContext _db;
    private readonly FakeHubContext _hub;
    private readonly EventRecorder _recorder;

    public EventsControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Single DI container owns the DatabaseContext; _db and the recorder's
        // per-event scopes all resolve from it against the same connection.
        var services = new ServiceCollection();
        services.AddDbContext<DatabaseContext>(options => options.UseSqlite(_connection));
        var provider = services.BuildServiceProvider();
        _scope = provider.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        _db.Database.EnsureCreated();

        _hub = new FakeHubContext();
        _recorder = new EventRecorder(_hub, provider.GetRequiredService<IServiceScopeFactory>());
    }

    public void Dispose()
    {
        _scope.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task PurgeAll_DeletesAllEvents_KeepsFilters()
    {
        var now = DateTime.UtcNow;

        var chatMessage = new ChatEventChatMessage
        {
            Id = Model.GetNewId<ChatEventChatMessage>(),
            Bits = 0,
            BitsInDollars = 0,
            Emotes = null,
            CustomRewardId = null,
            TwitchMessageId = "m1",
            IsBroadcaster = false,
            IsFirstMessage = false,
            IsHighlighted = false,
            IsMe = false,
            IsSkippingSubMode = false,
            Message = "hello",
            Noisy = TwitchLib.Client.Enums.Noisy.NotSet,
            SubscribedMonthCount = 0,
            TmiSent = now,
            ReplyParentMessageTwitchMessageId = null,
            DisplayName = "alice",
            UserId = "u1",
            Username = "alice",
            HexColor = "#fff",
            Badges = null,
            UserFlags = default,
            UserType = TwitchLib.Client.Enums.UserType.Viewer,
            Created = now,
            Updated = now,
        };
        _db.ChatEventChatMessages.Add(chatMessage);

        var joined = new ChatEventUserJoined
        {
            Id = Model.GetNewId<ChatEventUserJoined>(),
            Username = "bob",
            Channel = "testchannel",
            Created = now,
            Updated = now,
        };
        _db.ChatEventUserJoineds.Add(joined);

        var userNoticeBase = new ChatUserNoticeBase
        {
            Id = Model.GetNewId<ChatUserNoticeBase>(),
            HexColor = "#fff",
            DisplayName = "carol",
            Emotes = "",
            TwitchMessageId = "n1",
            Username = "carol",
            MsgId = "sub",
            RoomId = "123",
            SystemMsg = "carol subscribed",
            TmiSent = now,
            UserFlags = default,
            Badges = null,
            UserId = "u3",
            UserType = TwitchLib.Client.Enums.UserType.Viewer,
            Created = now,
            Updated = now,
        };
        _db.ChatUserNoticeBases.Add(userNoticeBase);

        var gift = new ChatEventGiftedSubscription
        {
            Id = Model.GetNewId<ChatEventGiftedSubscription>(),
            ChatUserNoticeBaseId = userNoticeBase.Id,
            IsAnonymous = false,
            MsgParamMonths = 1,
            MsgParamOriginId = "origin",
            MsgParamRecipientDisplayName = "carol",
            MsgParamRecipientId = "u3",
            MsgParamRecipientUserName = "carol",
            MsgParamSenderCount = 1,
            MsgParamSubPlan = TwitchLib.Client.Enums.SubscriptionPlan.Prime,
            MsgParamSubPlanName = "Prime",
            MsgParamMultiMonthGiftDuration = 0,
            Created = now,
            Updated = now,
        };
        _db.ChatEventGiftedSubscriptions.Add(gift);

        var filter = new ChatEventFilter
        {
            Id = Model.GetNewId<ChatEventFilter>(),
            Name = "keep-me",
            Code = "// code",
            CodeJs = "function __matches(e) { return true; }",
            Created = now,
            Updated = now,
        };
        _db.ChatEventFilters.Add(filter);

        _db.ChatEvents.AddRange(
            new ChatEvent { Id = Model.GetNewId<ChatEvent>(), ChatEventType = ChatEventType.ChatMessage, EventId = chatMessage.Id, Created = now, Updated = now },
            new ChatEvent { Id = Model.GetNewId<ChatEvent>(), ChatEventType = ChatEventType.UserJoined, EventId = joined.Id, Created = now, Updated = now },
            new ChatEvent { Id = Model.GetNewId<ChatEvent>(), ChatEventType = ChatEventType.GiftedSubscription, EventId = gift.Id, Created = now, Updated = now }
        );
        _db.SaveChanges();

        var controller = new ChatEventsController(_db, _hub, _recorder);
        var result = await controller.Delete();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(3, ok.Value?.GetType().GetProperty("deleted")?.GetValue(ok.Value));

        Assert.Equal(0, _db.ChatEvents.Count());
        Assert.Equal(0, _db.ChatEventChatMessages.Count());
        Assert.Equal(0, _db.ChatEventUserJoineds.Count());
        Assert.Equal(0, _db.ChatEventGiftedSubscriptions.Count());
        Assert.Equal(0, _db.ChatUserNoticeBases.Count());

        // Filters (and other non-event settings) are untouched.
        Assert.Equal(1, _db.ChatEventFilters.Count());
        Assert.True(_hub.Clients.AllProxy.SendCount >= 1);
    }

    [Fact]
    public async Task PurgeAll_OnEmptyDatabase_Succeeds()
    {
        var controller = new ChatEventsController(_db, _hub, _recorder);
        var result = await controller.Delete();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(0, ok.Value?.GetType().GetProperty("deleted")?.GetValue(ok.Value));
    }

    [Fact]
    public async Task GenerateTestData_CreatesOneEventOfEveryType_AndBroadcasts()
    {
        var controller = new ChatEventsController(_db, _hub, _recorder);
        var result = await controller.GenerateTestData();

        var ok = Assert.IsType<OkObjectResult>(result);
        var expected = Enum.GetValues<ChatEventType>().Count(t => t != ChatEventType.None);
        Assert.Equal(expected, ok.Value?.GetType().GetProperty("created")?.GetValue(ok.Value));
        Assert.Equal(expected, _db.ChatEvents.Count());
        Assert.Equal(expected, _hub.Clients.AllProxy.SendCount);
    }
}
