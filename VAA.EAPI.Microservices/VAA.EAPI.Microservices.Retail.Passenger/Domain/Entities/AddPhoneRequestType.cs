namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AddPhoneRequestType
    {
        public RequestInfoType RequestInfo { get; set; }
        public PhoneInfoType[] PhoneInfo { get; set; }
        public PnrRequestParamsType PnrRequestParams { get; set; }
        public RequestOptionType RequestOption { get; set; }
        public StampingInfoType StampingInfo { get; set; }
    }
}
