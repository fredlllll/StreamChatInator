namespace StreamChatInator
{
    public static class ReflectionHelper
    {
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
    }
}