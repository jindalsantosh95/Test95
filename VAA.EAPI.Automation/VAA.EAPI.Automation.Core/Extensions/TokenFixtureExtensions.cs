using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class TokenFixtureExtensions
    {
        public const string OauthToken = "/oauth/token";
        public const string V1OauthToken = "/v1/oauth/token";
        public const string OauthVRED2Token = "/oauth2/token";
        public const string Oauth2Token = "/6bdfd5a8-531f-4f14-9af1-dc7b444c3ad8/oauth2/token";
        public const string AzureB2CToken = "/42db763d-91b9-4d05-9811-6280695a5cae/oauth2/v2.0/token";
        public const string ChoooseToken = "/04300fc2-8f04-4555-9a5d-c6fac7f23d0c/oauth2/v2.0/token";
        public const string AtpcoToken = "/oauth/gettoken";
        public const string AwsTokenUrl = "https://ssaasi.vs01.vs.air4.com/as/token.oauth2";

        public static void AddTokenForGraphQlFromConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Constants.Environment.MsGqlClientId, Constants.Environment.MsGqlClientSecret, Constants.Environment.MsGqlResource);

        public static void AddTokenForGraphQlFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingGraphqlClientId", "TestingGraphqlClientSecret", "TestingGraphqlResource");

        public static void AddTokenForCustomerTxnFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingCustomerTxnMSClientId", "TestingCustomerTxnMSClientSecret", "TestingCustomerTxnMSResource");

        public static void AddTokenForCustomerMSFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingCustomerMSClientId", "TestingCustomerMSClientSecret", "TestingCustomerMSResource");

        public static void AddTokenForMicroservices(this TokenFixture source, string name, string resource) => source.Authorise(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Vault.Default.GetSecretValue("MYA-Common-Config-AutomationTestClientId"), Vault.Default.GetSecretValue("MYA-Common-Config-AutomationTestClientSecret"), resource);

        public static void AddTokenForBasketMicroservices(this TokenFixture source, string name, string resource) => source.Authorise(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Vault.Default.GetSecretValue(ConfigReader.Get(Constants.Environment.MicroBasketServiceClientId)), Vault.Default.GetSecretValue(ConfigReader.Get(Constants.Environment.MicroServiceBasketClientSecret)), resource);

        public static void AddTokenForMicroservicesFromKeyVault(this TokenFixture source, string name, string resource) => source.AddTokenForMicroservices(name, Vault.Default.GetSecretValue(resource));

        public static void AddTokenForDynamicsFromConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Constants.Environment.CrmClientId, Constants.Environment.CrmClientSecret, Constants.Environment.CrmResource);

        public static void AddTokenForDynamicsEnrolmentFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MYA-LoyaltyUpgradeClientId", "MYA-LoyaltyUpgradeClientSecret", "MYA-LoyaltyUpgradeClientResource");

        public static void AddTokenForDynamicsOrderSearchFromConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Constants.Environment.OrderSearchClientId, Constants.Environment.OrderSearchClientSecret, Constants.Environment.OrderSearchResource);

        public static void AddTokenForVRedFromKeyVault(this TokenFixture source, string name) => source.Authorise(name, Config.VredTokenCreationEndpoint.GetUrl(OauthVRED2Token), WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue("MYA-VredClientId"), Vault.Default.GetSecretValue("MYA-VredClientSecret"));

        public static void AddTokenForAzureB2CFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(AzureB2CToken), WebRequest.MultipartFormData.Post, "MYA-AzureB2CClientId", "MYA-AzureB2CClientSecret", null, null, "MYA-AzureB2CScope");

        public static void AddTokenForSOAFromConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.SoaBrand.GetUrl(V1OauthToken), WebRequest.FormUrlEncoded.Post, $"client_secret{Config.Environment}", $"client_id{Config.Environment}");

        public static void AddTokenForChooose(this TokenFixture source, string name) => source.Authorise(name, Config.MicrosoftOnline.GetUrl(ChoooseToken), WebRequest.MultipartFormData.Post, Vault.Default.GetSecretValue("ChoooseClientId"), Vault.Default.GetSecretValue("ChooseClientSecret"), scope: "https://partner-test.api.chooose.today/.default");

        public static void AddTokenForAtpco(this TokenFixture source, string name) => source.Authorise(name, Config.AtpcoGetToken.GetUrl(AtpcoToken), WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue(ConfigReader.Get(Constants.Environment.AtpcoClientId)), Vault.Default.GetSecretValue(ConfigReader.Get(Constants.Environment.AtpcoClientSecret)));

        public static void AddTokenForCustomerApiFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MarketingLoyalty-BackendCustomerAPIClientId-UAT", "MarketingLoyalty-BackendCustomerClientSecret-UAT", "MarketingLoyalty-BackendCustomerResource-UAT");

        public static void AddTokenForDeltaAPIFromKeyVault(this TokenFixture source, string name) => source.Authorise(name, Config.DeltaEndPoint.GetUrl(V1OauthToken), WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue("DeltaOfferClientId"), Vault.Default.GetSecretValue("DeltaOfferClientSecret"));

        public static void AddTokenForDynamicsIdentityFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MYA-LoyaltyUpgradeClientId", "MYA-LoyaltyUpgradeClientSecret", "MYA-IdentityResource");

        public static void AddTokenForReferenceApiFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MarketingLoyalty-BackendReferenceClientId-UAT", "MarketingLoyalty-BackendReferenceClientSecret-UAT", "MarketingLoyalty-BackendReferenceResource-UAT");

        public static void AddTokenForTransactionApiFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MarketingLoyalty-BackendTransactionClientId-UAT", "MarketingLoyalty-BackendTransactionClientSecret-UAT", "MarketingLoyalty-BackendTransactionResource-UAT");

        public static void AddTokenForAwardApiFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MarketingLoyalty-BackendAwardClientId-UAT", "MarketingLoyalty-BackendAwardClientSecret-UAT", "MarketingLoyalty-BackendAwardResource-UAT");

        public static void AddTokenForVirginPointsFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MYA-VirginPoints-ClientId", "MYA-VirginPoints-ClientSecret", "MYA-VirginPoints-Resource");

        public static void AddTokenForDeltaAwsAPIFromKeyVault(this TokenFixture source, string name) => source.Authorise(name, AwsTokenUrl, WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue("DeltaAwsClientId"), Vault.Default.GetSecretValue("DeltaAwsClientSecret"), scope: "read");

        public static void AddTokenForAwsAPIFromKeyVault(this TokenFixture source, string name) => source.Authorise(name, AwsTokenUrl, WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue("ZULU-ITliteClientId"), Vault.Default.GetSecretValue("ZULU-ITliteClientSecret"), scope: "read");

        public static void AddTokenForVRedMicroserviceFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingVRedMSClientId", "TestingVRedMSClientSecret", "TestingVRedMSResource");

        public static void AddTokenForAuditMicroserviceFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingAuditMSClientId", "TestingAuditMSClientSecret", "TestingAuditMSResource");

        public static void AddTokenForLoyaltyMemberMicroserviceFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingMbrMSClientId", "TestingMbrMSClientSecret", "TestingMbrMSResource");

        public static void AddTokenForCampaignMicroserviceFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "TestingCampaignMSClientId", "TestingCampaignMSClientSecret", "TestingCampaignMSResource");

        public static void AddTokenForFinancialPartnerApiFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MarketingLoyalty-BackendFinancialPartnerProcessingClientId", "MarketingLoyalty-BackendFinancialPartnerProcessingClientSecret", "MarketingLoyalty-BackendFinancialPartnerProcessingResource");

        public static void AddTokenForSkyTeamMicroserviceFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "MYA-SkyMs-ClientId", "MYA-SkyMs-ClientSecret", "MYA-SkyMs-Resource");

        public static void AddTokenForCalidrisApplicationFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Zulu-ApplicationClientId", "Zulu-ApplicationClientSecret", "Zulu-ApplicationResource");

        public static void AddTokenForCalidrisApplicationSitFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Zulu-ApplicationClientId-SIT", "Zulu-ApplicationClientSecret-SIT", "Zulu-ApplicationResource");

        public static void AddTokenForVirginHolidaysApplicationSitFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Zulu-ApplicationClientId-SIT", "Zulu-ApplicationClientSecret-SIT", "Zulu-ApplicationResourceVhols");

        public static void AddTokenForVirginHolidaysApplicationFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Zulu-ApplicationClientIdVhols", "Zulu-ApplicationClientSecretVhols", "Zulu-ApplicationResourceVhols");

        public static void AddTokenForDeltaAwsTRMAPIFromKeyVault(this TokenFixture source, string name) => source.Authorise(name, AwsTokenUrl, WebRequest.FormUrlEncoded.Post, Vault.Default.GetSecretValue("PCCOciWebClientId"), Vault.Default.GetSecretValue("DeltaAwsTRMAPIClientSecret"), scope: "read");

        public static void AddTokenForExploreHolidaysApplicationFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Explore-HolidaysGraphQL-ClientID", "Explore-HolidaysGraphQL-ClientSecret", "Explore-HolidaysGraphQL-Resource");

        public static void AddTokenForConciergeGraphQlRestFromKeyVault(this TokenFixture source, string name) => source.AuthoriseFromKeyVault(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, "Conceirge-GraphRest-ClientID", "Conceirge-GraphRest-ClientSecret", "Conceirge-GraphRest-Resource");

    }
}
