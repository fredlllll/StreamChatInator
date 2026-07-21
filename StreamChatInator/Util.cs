using TwitchLib.Client.Enums;

namespace StreamChatInator
{
    public static class Util
    {
        public static async Task RetryAsync(Func<bool>method,int tries, int delay=500, bool useExponentialBackoff=false, int maxDelay = 5000)
        {
            while(tries-- > 0)
            {
                if (method())
                {
                    return;
                }
                await Task.Delay(delay);
                if (useExponentialBackoff)
                {
                    delay *= 2;
                    if(delay > maxDelay)
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
    }
}
