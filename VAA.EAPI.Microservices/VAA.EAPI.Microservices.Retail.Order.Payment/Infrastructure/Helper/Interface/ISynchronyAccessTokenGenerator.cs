using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface
{
    public interface ISynchronyAccessTokenGenerator
    {
        Task<string> GetToken();
    }
}
