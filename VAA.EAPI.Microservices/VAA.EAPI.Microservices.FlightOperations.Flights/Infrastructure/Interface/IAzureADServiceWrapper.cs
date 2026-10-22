namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface
{
    public interface IAzureADServiceWrapper
    {
        Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource);
        Task<string> GetAccessTokenV2(string clientId, string clientSecret, string authority, string scope);
    }
}
