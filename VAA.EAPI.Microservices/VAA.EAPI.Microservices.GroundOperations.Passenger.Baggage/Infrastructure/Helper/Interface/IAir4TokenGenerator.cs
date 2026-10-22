namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Infrastructure.Helper.Interface
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
