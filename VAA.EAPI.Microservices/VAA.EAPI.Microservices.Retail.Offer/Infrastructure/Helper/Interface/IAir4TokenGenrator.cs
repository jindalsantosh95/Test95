using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface
{
    public interface IAir4TokenGenerator
    {
        Task<string> GetToken(string clientId, string clientSecret, bool switchToAws);
    }
}
