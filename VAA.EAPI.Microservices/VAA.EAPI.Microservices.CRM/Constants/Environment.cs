namespace VAA.EAPI.Microservices.CRM.Constants
{
    public class Environment
    {
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";

        public static readonly string CRMWebApiEndpoint = "CRMWebApi:Endpoint";
        public static readonly string CRMWebApiGetEndpointAPIM = "CRMWebApi:GetEndpointAPIM";
        public static readonly string CRMWebApiPatchEndpointAPIM = "CRMWebApi:PatchEndpointAPIM";
        public static readonly string CRMWebApiOCPSubKey = "CRMWebApi:OcpSubKey";
        public static readonly string CRMWebApiVersion = "CRMWebApi:Version";
        public static readonly string CRMWebApiResource = "CRMWebApi:Resource";
        public static readonly string CRMWebApiPaymentFilterParams = "CRMWebApi:PaymentFilterParams";
        public static readonly string CallCRMAPIMUrl = "CRMWebApi:CallCRMAPIMUrl";
        public static readonly string CRMWebApiCountryFilterParams = "CRMWebApi:CountryFilterParams";
        public static readonly string CRMWebApiResponsiveFeature = "CRMWebApi:ResponsiveFeature";
        public const string CRMWebApiPaymentUpdateResponseUrl = "CRMWebApi:PaymentUpdateResponseUrl";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";

        public static readonly string AzureAdClientId = "AzureAd:ClientId";
        public static readonly string AzureAdClientSecret = "AzureAd:ClientSecret";


        public static readonly string CRMPaymentEventHandlerTopicName = "CRMPaymentEventHandlerTopicName";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string AzureAdInstance = "AzureAd:Instance";
        public static readonly string AzureAdTenantId = "AzureAd:TenantId";

    }
}
