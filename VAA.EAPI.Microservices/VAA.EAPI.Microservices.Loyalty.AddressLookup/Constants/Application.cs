namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants
{
    public static class Application
    {
        public static readonly string Success = "SUCCESS";
        public static readonly string DataVersion = "0.1";
        public static readonly string Subject = "Enrolment";
        public const string AddressLookupEventHandlerTopicName = "enrolcustomer";
        public const string AddressLookupEventSubscriptionName = "EnrolCustomerAddressLookupSubscriber";
        public const string OcpKeyHeader = "Ocp-APIM-Subscription-Key";
        public const string AzureEndpointContentType = "text/xml";

    }
}
