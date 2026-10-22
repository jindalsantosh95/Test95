using System;
using System.Threading.Tasks;
using VAA.Facade.AzureB2C.Infrastructure.ExternalServices;

namespace VAA.Facade.AzureB2C.Shared
{
    public static class AzureADHelper
    {
        /// <summary>
        /// Bearer token with auto refresh
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetBearerToken(bool reset)
        {
            if (Constants.EnrolmentAPITokenGenerated == null || reset)
            {
                #region Check if ClientID value is available in static Field
                if (Constants.EnrolmentClientIDCached == null)
                {
                    Constants.EnrolmentClientIDCached = ReadConfig.GetConfigurationSettings(Constants.EnrolmentClientID);
                }
                #endregion

                #region Check if Secret value is available in static Field
                if (Constants.EnrolmentClientSecretValueCached == null)
                {
                    Constants.EnrolmentClientSecretValueCached = ReadConfig.GetConfigurationSettings(Constants.EnrolmentClientSecretValue);
                }
                #endregion

                var bearerToken = await AzureADService
                                        .GetAccessToken(
                                Constants.EnrolmentClientIDCached,
                                Constants.EnrolmentClientSecretValueCached,
                                ReadConfig.GetConfigurationSettings(Constants.azureADEndpoint),
                                ReadConfig.GetConfigurationSettings(Constants.EnrolmentResource)
                                                   );
                Constants.EnrolmentAPITokenGenerated = bearerToken;
                return bearerToken;
            }
            else
            {
                return Constants.EnrolmentAPITokenGenerated;
            }
        }

       
    }

}
