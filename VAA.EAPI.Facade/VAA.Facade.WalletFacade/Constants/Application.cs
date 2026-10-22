namespace VAA.Facade.WalletFacade.Constants
{
    public class Application
    {
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static string RegisterPassEndpoint = "RegisterPass";
        public static string DeletePassEndpoint = "DeletePass";
        public static string GetPassEndpoint = "GetUpdatedPasses/{deviceId}/{passTypeId}";
        public static string SendUpdatedPassEndpoint = "SendUpdatedPass/{serialNumber}";
        public static string MediaType = "application/octet-stream";
        public static string ContentDisposition = "Content-Disposition";
        public static string AccessControlHeader = "Access-Control-Expose-Headers";

    }
}
