using StreamChatInator.Database.Models;
using System.Text.Json;
using TwitchLib.Client.Enums;

namespace StreamChatInator
{
    public static class Util
    {
        public static async Task RetryAsync(Func<bool> method, int tries, int delay = 500, bool useExponentialBackoff = false, int maxDelay = 5000)
        {
            while (tries-- > 0)
            {
                if (method())
                {
                    return;
                }
                await Task.Delay(delay);
                if (useExponentialBackoff)
                {
                    delay *= 2;
                    if (delay > maxDelay)
                    {
                        delay = maxDelay;
                        useExponentialBackoff = false;
                    }
                }
            }
            throw new TimeoutException("Failed calling method even with retries");
        }

        public static T GetPrivateFieldNotNull<T>(object target, string fieldName)
        {
            var t = target.GetType();
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                throw new Exception($"private field {fieldName} does not exist on type {t.Name}");
            }
            else
            {
                return (T)(field.GetValue(target) ?? throw new InvalidDataException($"{fieldName} field returned null"));
            }
        }

        public static string[] FlagEnumNames(System.Enum value)
        {
            return value
            .ToString()
            .Split(", ", StringSplitOptions.RemoveEmptyEntries)
            .Where(f => f != "None")
            .ToArray();
        }

        public static dynamic MergeObjects(JsonNamingPolicy? namingPolicy = null, params object[] objects)
        {
            // Merge public properties/fields and dictionary entries from multiple objects
            // into a single ExpandoObject. Later objects overwrite earlier values for the same key.
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }
            if (namingPolicy == null)
            {
                namingPolicy = new JsonNoChangeNamingPolicy();
            }

            var result = new System.Dynamic.ExpandoObject();
            var resultDict = (IDictionary<string, object?>)result;

            foreach (var obj in objects)
            {
                if (obj is null)
                    continue;

                // If object is a generic dictionary of string->object, copy entries
                if (obj is IDictionary<string, object> genDict)
                {
                    foreach (var kvp in genDict)
                    {
                        var name = namingPolicy.ConvertName(kvp.Key);
                        resultDict[name] = kvp.Value;
                    }

                    continue;
                }

                // If object is a non-generic IDictionary, copy entries (keys converted to string)
                if (obj is System.Collections.IDictionary dict)
                {
                    foreach (System.Collections.DictionaryEntry entry in dict)
                    {
                        var key = entry.Key?.ToString() ?? string.Empty;
                        key = namingPolicy.ConvertName(key);
                        resultDict[key] = entry.Value;
                    }

                    continue;
                }

                // Otherwise use reflection to copy public instance properties and fields
                var type = obj.GetType();

                // Copy public readable properties (skip indexers)
                var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var prop in properties)
                {
                    if (!prop.CanRead)
                        continue;

                    if (prop.GetIndexParameters().Length > 0)
                        continue; // skip indexers

                    var name = namingPolicy.ConvertName(prop.Name);
                    var value = prop.GetValue(obj);
                    resultDict[name] = value;
                }

                // Copy public instance fields
                var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var field in fields)
                {
                    var name = namingPolicy.ConvertName(field.Name);
                    var value = field.GetValue(obj);
                    resultDict[name] = value;
                }
            }

            return result;
        }

        public static FrontEndEventData ToFrontendData(ChatEvent chatEvent, Model eventData)
        {
            return new FrontEndEventData
            {
                EventId = chatEvent.Id,
                ChatEventType = chatEvent.ChatEventType,
                ChatEventData = eventData
            };
        }

        public static FrontEndEventData ToFrontendData(ChatEvent chatEvent, Model eventData, Model eventSubData)
        {
            return new FrontEndEventData
            {
                EventId = chatEvent.Id,
                ChatEventType = chatEvent.ChatEventType,
                ChatEventData = MergeObjects(JsonNamingPolicy.CamelCase, eventSubData, eventData)
            };
        }
    }
}
