namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserResponse
{
    public class UserGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool BuiltIn { get; set; }
        public string Type { get; set; }
        public string ExternalId { get; set; }
    }
}
