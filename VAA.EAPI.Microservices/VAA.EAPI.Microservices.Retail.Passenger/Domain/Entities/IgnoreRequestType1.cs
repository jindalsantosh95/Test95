namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class IgnoreRequestType1
    {
        public RequestInfoType RequestInfo { get; set; }
        public StampingInfoType StampingInfo { get; set; }
        public PnrRequestParamsType PnrRequestParams { get; set; }
        public string RedisplayFlag { get; set; }
        public RequestOptionType RequestOptions { get; set; }
    }
}
