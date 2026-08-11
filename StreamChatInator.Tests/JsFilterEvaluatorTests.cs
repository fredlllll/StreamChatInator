using StreamChatInator.Database.Models;

namespace StreamChatInator.Tests;

public class JsFilterEvaluatorTests
{
    private static FrontEndEventData Event(string message) => new()
    {
        EventId = "evt_1",
        ChatEventType = ChatEventType.ChatMessage,
        ChatEventData = new { message },
    };

    [Fact]
    public void ScriptWithoutMatches_LetsEverythingThrough()
    {
        var evaluator = new JsFilterEvaluator("var x = 1;");
        Assert.True(evaluator.Matches(Event("hello")));
    }

    [Fact]
    public void EmptyScript_LetsEverythingThrough()
    {
        var evaluator = new JsFilterEvaluator("");
        Assert.True(evaluator.Matches(Event("hello")));
    }

    [Fact]
    public void ScriptMatchingMessage_ReturnsMatchResult()
    {
        var evaluator = new JsFilterEvaluator(
            "function __matches(eventData) { return eventData.chatEventData.message === 'hello'; }");

        Assert.True(evaluator.Matches(Event("hello")));
        Assert.False(evaluator.Matches(Event("world")));
    }

    [Fact]
    public void ScriptUsingExpression_BodyEvaluates()
    {
        var evaluator = new JsFilterEvaluator(
            "function __matches(eventData) { return eventData.chatEventData.message.length > 5; }");

        Assert.True(evaluator.Matches(Event("a very long message")));
        Assert.False(evaluator.Matches(Event("short")));
    }

    [Fact]
    public void ThrowingScriptDuringConstruction_DefaultsToPass()
    {
        var evaluator = new JsFilterEvaluator("throw new Error('bad script');");
        Assert.True(evaluator.Matches(Event("anything")));
    }

    [Fact]
    public void ThrowingScriptDuringEvaluation_ReturnsTrue()
    {
        var evaluator = new JsFilterEvaluator(
            "function __matches(eventData) { return eventData.nonexistent.deep.value; }");

        Assert.True(evaluator.Matches(Event("anything")));
    }
}
