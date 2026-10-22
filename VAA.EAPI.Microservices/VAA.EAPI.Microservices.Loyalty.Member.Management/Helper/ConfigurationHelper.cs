using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper
{
    public class ConfigurationHelper
    {
        public async static Task<string> GetBearerToken()
        {
            string clientId = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtAppId);
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            Constants.Application.LoyaltyMbrMgmtClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.LoyaltyMbrMgmtClientSecret, Constants.Application.LoyaltyMbrMgmtClientSecretCached);
            Constants.Application.CustomerTxnMSResourceIdCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.CustomerTxnMSResourceId, Constants.Application.CustomerTxnMSResourceIdCached);
            string token = await ExternalHelper.GetBearerToken(clientId, Constants.Application.LoyaltyMbrMgmtClientSecretCached, authority, Constants.Application.CustomerTxnMSResourceIdCached);
            return token;
        }
        public static Dictionary<string, string> GetHeader()
        {
            Constants.Application.LoyaltyMbrMgmtOcpKeyCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.LoyaltyMbrMgmtOcpKey, Constants.Application.LoyaltyMbrMgmtOcpKeyCached);
            Dictionary<string, string> headers = new()
            {
                { Constants.Environment.OcpKeyHeader, Constants.Application.LoyaltyMbrMgmtOcpKeyCached }
            };
            return headers;
        }

        public async static Task<string> GetBearerTokenCustomerMS()
        {
            string clientId = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtAppId);
            string authority = ConfigReader.Get(Constants.Environment.AzureADEndpoint);
            Constants.Application.LoyaltyMbrMgmtClientSecretCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.LoyaltyMbrMgmtClientSecret, Constants.Application.LoyaltyMbrMgmtClientSecretCached);
            Constants.Application.CustomerMSResourceIdCached = ExternalHelper.GetSecretsManagedIDentity(Constants.Environment.CustomerMSAppId, Constants.Application.CustomerMSResourceIdCached);
            return await ExternalHelper.GetBearerToken(clientId, Constants.Application.LoyaltyMbrMgmtClientSecretCached, authority, Constants.Application.CustomerMSResourceIdCached);
        }
    }
}
