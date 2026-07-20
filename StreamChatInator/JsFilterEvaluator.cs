using Jint;
using System.Text.Json;

namespace StreamChatInator
{
    public class JsFilterEvaluator
    {
        private readonly Engine _engine;
        private readonly JsonSerializerOptions _jsonOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public JsFilterEvaluator(string code)
        {
            _engine = new Engine();
            _engine.Execute($"function __matches(eventType, eventData) {{ {code} }}");
        }

        public bool Matches(string eventType, object eventData)
        {
            var json = JsonSerializer.Serialize(eventData, _jsonOptions);
            _engine.SetValue("__eventType", eventType);
            _engine.SetValue("__eventDataJson", json);
            _engine.Execute("var __eventData = JSON.parse(__eventDataJson);");

            try
            {
                return _engine.Evaluate("__matches(__eventType, __eventData)").AsBoolean();
            }
            catch
            {
                return false;
            }
        }
    }
}