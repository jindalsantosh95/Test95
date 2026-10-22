namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AddPTCCodesRequestType
    {
        public RequestInfoType RequestInfo { get; set; }
        public PTCCodeType[] PTCCode { get; set; }
        public PnrRequestParamsType PnrRequestParams { get; set; }
        public RequestOptionType RequestOption { get; set; }
        public StampingInfoType StampingInfo { get; set; }
    }
}
