using StreamChatInator.Database.Models;

namespace StreamChatInator.Tests;

public class ModelTests
{
    [Fact]
    public void GetNewId_IsPrefixedWithLowercaseTypeName()
    {
        var id = Model.GetNewId<ChatEventFilter>();
        Assert.StartsWith("chateventfilter_", id);
    }

    [Fact]
    public void GetNewId_ProducesUniqueIds()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => Model.GetNewId<ChatEventFilter>()).ToList();
        Assert.Equal(100, ids.Distinct().Count());
    }

    [Fact]
    public void Equals_ComparesIdCreatedAndUpdated()
    {
        var t = DateTime.UtcNow;
        var a = new ChatEventFilter { Id = "f_1", Name = "n", Code = "c", CodeJs = "c", Created = t, Updated = t };
        var b = new ChatEventFilter { Id = "f_1", Name = "n", Code = "c", CodeJs = "c", Created = t, Updated = t };
        var c = new ChatEventFilter { Id = "f_2", Name = "n", Code = "c", CodeJs = "c", Created = t, Updated = t };

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.True(a == b);
        Assert.True(a != c);
    }
}
