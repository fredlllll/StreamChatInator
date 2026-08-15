using System.Text.Json;

namespace StreamChatInator
{
    /// <summary>
    /// Merges public properties/fields and dictionary entries from multiple
    /// objects into a single dynamic object. Later objects overwrite earlier
    /// values for the same key.
    /// </summary>
    public static class ObjectMerger
    {
        public static dynamic MergeObjects(JsonNamingPolicy? namingPolicy = null, params object[] objects)
        {
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
    }
}