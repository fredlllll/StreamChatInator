using StreamChatInator.Database.Models;
using System.Text.Json;

namespace StreamChatInator.Tests;

public class HelperTests
{
    [Flags]
    private enum TestFlags
    {
        None = 0,
        Alpha = 1,
        Beta = 2,
        Gamma = 4,
    }

    [Fact]
    public void FlagEnumNames_SplitsAndExcludesNone()
    {
        var names = EnumHelper.FlagEnumNames(TestFlags.Alpha | TestFlags.Gamma);
        Assert.Equal(new[] { "Alpha", "Gamma" }, names);
    }

    [Fact]
    public void FlagEnumNames_ReturnsEmpty_ForNone()
    {
        Assert.Empty(EnumHelper.FlagEnumNames(TestFlags.None));
    }

    [Fact]
    public void SerializeBadges_ReturnsNull_ForNullOrEmpty()
    {
        Assert.Null(BadgeSerializer.SerializeBadges(null));
        Assert.Null(BadgeSerializer.SerializeBadges(new List<KeyValuePair<string, string>>()));
    }

    [Fact]
    public void SerializeBadges_DefaultsMissingVersionToZero()
    {
        var json = BadgeSerializer.SerializeBadges(new List<KeyValuePair<string, string>>
        {
            new("broadcaster", "1"),
            new("subscriber", ""),
        });

        Assert.Equal("[{\"set\":\"broadcaster\",\"version\":\"1\"},{\"set\":\"subscriber\",\"version\":\"0\"}]", json);
    }

    [Fact]
    public void SerializeBadges_FiltersBlankSetKeys()
    {
        var json = BadgeSerializer.SerializeBadges(new List<KeyValuePair<string, string>>
        {
            new("", "9"),
            new("vip", "1"),
        });

        Assert.Equal("[{\"set\":\"vip\",\"version\":\"1\"}]", json);
    }

    [Fact]
    public void MergeObjects_LaterValuesOverwriteEarlier()
    {
        var result = (IDictionary<string, object?>)ObjectMerger.MergeObjects(
            objects: new object[] { new Dictionary<string, object> { ["foo"] = 1 }, new { foo = 2, bar = "b" } });

        Assert.Equal(2, result["foo"]);
        Assert.Equal("b", result["bar"]);
    }

    [Fact]
    public void MergeObjects_AppliesCamelCaseNamingPolicy()
    {
        var result = (IDictionary<string, object?>)ObjectMerger.MergeObjects(JsonNamingPolicy.CamelCase, new { SomeProperty = 42 });
        Assert.Equal(42, result["someProperty"]);
    }

    [Fact]
    public void MergeObjects_SkipsNullEntries()
    {
        var result = (IDictionary<string, object?>)ObjectMerger.MergeObjects(objects: new object[] { new { a = 1 }, null! });
        Assert.Single(result);
        Assert.Equal(1, result["a"]);
    }

    [Fact]
    public void MergeObjects_Throws_WhenAllObjectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ObjectMerger.MergeObjects(null, (object[])null!));
    }

    [Fact]
    public void ToFrontendData_MapsChatEvent()
    {
        var chatEvent = new ChatEvent
        {
            Id = "evt_1",
            ChatEventType = ChatEventType.UserJoined,
            EventId = "uj_1",
            Seen = true,
        };
        var data = new ChatEventUserJoined { Id = "uj_1", Username = "alice", Channel = "c" };

        var frontend = FrontEndEventMapper.ToFrontendData(chatEvent, data);

        Assert.Equal("evt_1", frontend.EventId);
        Assert.Equal(ChatEventType.UserJoined, frontend.ChatEventType);
        Assert.True(frontend.Seen);
        Assert.Same(data, frontend.ChatEventData);
    }

    private class BaseSub : Model
    {
        public string? Value { get; set; }
    }

    private class DetailModel : Model
    {
        public string? Detail { get; set; }
    }

    [Fact]
    public void ToFrontendData_MergesSubDataThenData_CamelCased()
    {
        var chatEvent = new ChatEvent
        {
            Id = "evt_1",
            ChatEventType = ChatEventType.Announcement,
            EventId = "detail_1",
        };
        var subData = new BaseSub { Id = "sub_1", Value = "base" };
        var detail = new DetailModel { Id = "detail_1", Detail = "extra" };

        var frontend = FrontEndEventMapper.ToFrontendData(chatEvent, detail, subData);
        var dict = (IDictionary<string, object?>)frontend.ChatEventData;

        Assert.Equal("base", dict["value"]);
        Assert.Equal("extra", dict["detail"]);
    }

    private class FieldHolder
    {
#pragma warning disable CS0414 // read via reflection in GetPrivateFieldNotNull_ReadsPrivateField
        private readonly string _secret = "hello";
#pragma warning restore CS0414
    }

    [Fact]
    public void GetPrivateFieldNotNull_ReadsPrivateField()
    {
        var holder = new FieldHolder();
        Assert.Equal("hello", ReflectionHelper.GetPrivateFieldNotNull<string>(holder, "_secret"));
    }

    [Fact]
    public void GetPrivateFieldNotNull_Throws_WhenFieldMissing()
    {
        var holder = new FieldHolder();
        Assert.Throws<Exception>(() => ReflectionHelper.GetPrivateFieldNotNull<string>(holder, "nope"));
    }

    [Fact]
    public async Task RetryAsync_RetriesUntilSuccess()
    {
        var attempts = 0;
        await RetryHelper.RetryAsync(() =>
        {
            attempts++;
            return attempts >= 3;
        }, tries: 5, delay: 1);

        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task RetryAsync_ThrowsTimeout_WhenExhausted()
    {
        await Assert.ThrowsAsync<TimeoutException>(() => RetryHelper.RetryAsync(() => false, tries: 2, delay: 1));
    }
}
