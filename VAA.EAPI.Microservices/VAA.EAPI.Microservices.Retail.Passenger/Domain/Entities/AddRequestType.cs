namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AddRequestType
    {
        public RequestInfoType RequestInfo { get; set; }
        public PassengerInfoType[] PassengerInfo { get; set; }
        public PnrRequestParamsType PnrRequestParams { get; set; }
        public RequestOptionType RequestOption { get; set; }
        public StampingInfoType StampingInfo { get; set; }
    }
}
