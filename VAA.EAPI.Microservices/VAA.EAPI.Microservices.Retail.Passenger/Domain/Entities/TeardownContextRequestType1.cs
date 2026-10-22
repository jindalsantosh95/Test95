namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class TeardownContextRequestType1
    {
        public RequestInfoType RequestInfo { get; set; }
        public StampingInfoType StampingInfo { get; set; }
        public EndRequestType1 EndRequest { get; set; }
        public IgnoreRequestType1 IgnoreRequest { get; set; }
        public RefreshRequestType1 RefreshRequest { get; set; }
        public string DoNotDestroySessionFlag { get; set; }
    }
}
