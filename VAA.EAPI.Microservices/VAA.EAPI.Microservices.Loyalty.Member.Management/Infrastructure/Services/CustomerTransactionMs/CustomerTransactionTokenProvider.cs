using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs
{
    public class CustomerTransactionTokenProvider : ICustomerTransactionTokenProvider
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;

        public CustomerTransactionTokenProvider(IOAuthTokenProvider oauthTokenProvider)
        {
            _oauthTokenProvider = oauthTokenProvider ??
                throw new System.ArgumentNullException(nameof(oauthTokenProvider));
        }

        public Task<string> AcquireTokenAsync() =>
            _oauthTokenProvider.AcquireTokenAsync(
                ConfigReader.Get(Constants.Environment.CustomerTxnMSResourceId));
    }
}
