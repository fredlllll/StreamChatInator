namespace StreamChatInator
{
    public static class EnumHelper
    {
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