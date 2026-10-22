using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Security.Constants;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Helper
{
    public class APIMSubscriptionConfigHelper
    {
        public static APIMSubscriptionConfig GetAPIMConfiguration(string channel, string enviornment)
        {
            string apimConfigJson = ConfigReader.ReadAllText(VAA.EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Application.APIMSubscriptionConfigFilePath);
            List<APIMSubscriptionConfig> apimConfigurations = Json.Deserialize<List<APIMSubscriptionConfig>>(apimConfigJson);
            APIMSubscriptionConfig apimConfig = apimConfigurations.Where(apimConfig => apimConfig.Channel.Equals(channel) && apimConfig.Environment.Equals(enviornment)).FirstOrDefault();
            return apimConfig;
        }

        public static string GetSubscriptionURI(APIMSubscriptionConfig apimConfig, string subscriptionID)
        {
            string subscriptionAPIBaseUrl = Constants.Application.MSSubscriptionAPIBaseUrl;
            string apimSubscriptionUri = Constants.Application.APIMSubscriptionUri;
            return subscriptionAPIBaseUrl + string.Format(apimSubscriptionUri, apimConfig.APIMSubscriptionId, apimConfig.APIMResourceGroupName, apimConfig.APIMName, subscriptionID);

        }

        public static string GetSubscriptionKeysURI(APIMSubscriptionConfig apimConfig, string subscriptionID)
        {
            string subscriptionAPIBaseUrl = Constants.Application.MSSubscriptionAPIBaseUrl;
            string apimSubscriptionKeysUri = Constants.Application.APIMSubscriptionKeysUri;
            return subscriptionAPIBaseUrl + string.Format(apimSubscriptionKeysUri, apimConfig.APIMSubscriptionId, apimConfig.APIMResourceGroupName, apimConfig.APIMName, subscriptionID);

        }

        public static string GetCreateAPIMUserURI(APIMSubscriptionConfig apimConfig, string userID)
        {
            string subscriptionAPIBaseUrl = Constants.Application.MSSubscriptionAPIBaseUrl;
            string createAPIMUserUri = Constants.Application.CreateAPIMUserUri;
            return subscriptionAPIBaseUrl + string.Format(createAPIMUserUri, apimConfig.APIMSubscriptionId, apimConfig.APIMResourceGroupName, apimConfig.APIMName, userID);
        }
    }
}
