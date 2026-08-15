namespace StreamChatInator.ApiModels
{
    public class UpsertFilterRequest
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public required string CodeJs { get; set; }
    }
}