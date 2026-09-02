using Jint;
using Jint.Runtime;
using System.Text.Json;

namespace StreamChatInator
{
    public class JsFilterEvaluator
    {
        private readonly Engine _engine;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _jsonOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public JsFilterEvaluator(string code)
        {
            _engine = new Engine();
            // The stored script defines `__matches(eventData)`.
            try
            {
                _engine.Execute(code);
            }
            catch (JavaScriptException)
            {
                //ignore code that causes exception. filter will just return true if this is the case
            }
        }
        public bool Matches(FrontEndEventData eventData)
        {
            var json = JsonSerializer.Serialize(eventData, _jsonOptions);
            // A Jint Engine is not thread-safe and cached evaluators are shared
            // across requests, so serialize evaluations per engine.
            lock (_lock)
            {
                _engine.SetValue("__eventDataJson", json);
                _engine.Execute("var __eventData = JSON.parse(__eventDataJson);");

                try
                {
                    // Run the filter's `__matches`; if the script didn't define it,
                    // let the event through (default true). `typeof` on an undeclared
                    // identifier is safe (no ReferenceError), unlike a bare reference.
                    return _engine.Evaluate("typeof __matches === 'function' ? __matches(__eventData) : true").AsBoolean();
                }
                catch
                {
                    // Fail-open on runtime errors: a broken filter shows everything
                    // rather than nothing, which is more noticeable in the UI.
                    // TODO: surface filter errors to the frontend (e.g. an "error"
                    // state on the filter) so broken code is flagged instead of
                    // silently broadening to "match all". Deliberately deferred to
                    // avoid a larger schema/API change right before release.
                    return true;
                }
            }
        }
    }
}