namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class RequestInfoType
    {
        public HostContextParamsType HostContextParams { get; set; }
        public DBConnectionInfoType DBConnectionInfo { get; set; }
        public string AppChannelName { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationVersion { get; set; }
        public string Company { get; set; }
        public string DebugOptions { get; set; }
        public string Description { get; set; }
        public string ExternalVendorFlag { get; set; }
        public string Location { get; set; }
        public string SessionId { get; set; }
        public string TestLab { get; set; }
        public string TransactionId { get; set; }
        public string TurnOffResponseInfo { get; set; }
    }
}
