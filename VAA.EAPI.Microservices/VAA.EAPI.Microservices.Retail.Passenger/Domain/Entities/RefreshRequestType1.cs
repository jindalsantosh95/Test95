namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class RefreshRequestType1
    {
        public RequestInfoType RequestInfo { get; set; }

        public StampingInfoType StampingInfo { get; set; }

        public PnrRequestParamsType PnrRequestParams { get; set; }

        public RequestOptionType RequestOptions { get; set; }
    }
}
