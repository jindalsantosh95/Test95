using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
