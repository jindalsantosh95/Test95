namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
