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
        var timestamp = DateTime.UtcNow;
        var first = new ChatEventFilter { Id = "f_1", Name = "n", Code = "c", CodeJs = "c", Created = timestamp, Updated = timestamp };
        var sameValues = new ChatEventFilter { Id = "f_1", Name = "n", Code = "c", CodeJs = "c", Created = timestamp, Updated = timestamp };
        var differentId = new ChatEventFilter { Id = "f_2", Name = "n", Code = "c", CodeJs = "c", Created = timestamp, Updated = timestamp };

        Assert.Equal(first, sameValues);
        Assert.NotEqual(first, differentId);
        Assert.True(first == sameValues);
        Assert.True(first != differentId);
    }
}
