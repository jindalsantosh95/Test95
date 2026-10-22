using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs
{
    public interface ICustomerTransactionTokenProvider
    {
        Task<string> AcquireTokenAsync();
    }
}
