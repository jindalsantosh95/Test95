namespace VAA.EAPI.Microservices.CRM.Constants
{
    public class Application
    {
        //CRM DCE Web API
        public const string HttpVerb = "Patch";
        public static readonly string CRMCountryEntity = "vaa_countries";
        public static readonly string CRMPaymentEntity = "vaa_payments";

        public static readonly string EventSpecversion = "1.0";
        public static readonly string CRMPaymentUpdateEventSubject = "CRMPaymentUpdate";

        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string OcpKeyHeader = "Ocp-APIM-Subscription-Key";

        public static readonly string HeaaderParameterlocation = "location";
        public static readonly int SentForPaymentStage = 806180007;
    }
}
