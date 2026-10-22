namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserResponse
{
    public class UserCreationResponse
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public UserProperties Properties { get; set; }
    }
}
