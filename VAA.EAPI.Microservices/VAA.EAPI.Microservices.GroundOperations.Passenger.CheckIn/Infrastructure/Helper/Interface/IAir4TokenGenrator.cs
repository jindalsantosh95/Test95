using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
