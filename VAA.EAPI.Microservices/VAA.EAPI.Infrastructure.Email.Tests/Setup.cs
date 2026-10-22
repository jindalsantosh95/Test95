using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Email.Tests
{
    public sealed class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ACMSessionToken", "TestToken"},
                {"ACMEmailTemplate", "TestTemplate"},
                {"ACMCommunicationChannel", "TestChannelId"},
                {"EnableAdobeNewEndpoint", "false"},
                {"SoapEndpointUri","https://virginatlantic-rt-stage2.campaign.adobe.com/nl/jsp/soaprouter.jsp"},
                {"SoapEndpointUriV2","http://tempuri.org"},
                {"ClientId","testvalue"},
                { "ClientSecret","testvalue"},
                { "GrantType", "client_credentials" },
                { "DelayTime", "0:0:0:5" },
                { "AuthenticationUrl","https://ims-na1.adobelogin.com/ims/" },
                { "EnableEmailMSAuthentication", "true" },
                { "Scope", "AdobeID,openid,read_organizations,additional_info.projectedProductContext,additional_info.roles,adobeio_api,read_client_secret,manage_client_secrets,campaign_sdk,campaign_config_server_general,deliverability_service_general" },
                {"AuthorizationTokenType","Bearer" }

            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
