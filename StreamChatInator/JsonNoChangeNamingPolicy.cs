using System.Text.Json;

namespace StreamChatInator
{
    public class JsonNoChangeNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name;
        }
    }
}
