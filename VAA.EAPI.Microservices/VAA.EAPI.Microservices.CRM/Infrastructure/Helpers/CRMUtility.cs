using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.CRM.Infrastructure.Helpers
{
    public class CRMUtility
    {
        public static string GetVAAOrgCRMURI()
        {
            return $"{ConfigReader.Get(Constants.Environment.CRMWebApiEndpoint)}/api/data/v{ConfigReader.Get(Constants.Environment.CRMWebApiVersion)}";
        }

        public static async Task<string> GetCRMToken()
        {
            string clientId = ConfigReader.Get(Constants.Environment.AzureAdClientId);
            string clientSecret = ConfigReader.Get(Constants.Environment.AzureAdClientSecret);
            string authority = $"{ConfigReader.Get(Constants.Environment.AzureAdInstance)}/{ConfigReader.Get(Constants.Environment.AzureAdTenantId)}";
            string resource = ConfigReader.Get(Constants.Environment.CRMWebApiEndpoint);

            return await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
        }

        public static async Task<Dictionary<string, string>> GetHeaders()
        {
            return await Task.FromResult(new Dictionary<string, string>()
            {
                {
                     Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.CRMWebApiOCPSubKey)
                }
            });
        }
        public static string CRMWebAPIGETAPIMURL(string crmEntityName)
        {
            return ConfigReader.Get(Constants.Environment.CRMWebApiGetEndpointAPIM)
                    .Replace("{crm_entity}", crmEntityName);
        }

        public static string CRMWebAPIPatchAPIMURL(string crmEntityName, string entityId)
        {
            return ConfigReader.Get(Constants.Environment.CRMWebApiPatchEndpointAPIM)
                    .Replace("{crm_entity}", crmEntityName).Replace("{entityId}", entityId);
        }
    }
}
