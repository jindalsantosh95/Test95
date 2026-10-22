namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class SetupContextRequestType1
    {
        public RequestInfoType RequestInfo { get; set; }
        public StampingInfoType StampingInfo { get; set; }
        public RequestOptionType RequestOptions { get; set; }
        public string RecordLocator { get; set; }
    }
}
