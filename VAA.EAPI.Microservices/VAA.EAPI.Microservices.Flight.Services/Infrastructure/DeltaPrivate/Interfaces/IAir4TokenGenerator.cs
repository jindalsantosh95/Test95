using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate.Interfaces
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
