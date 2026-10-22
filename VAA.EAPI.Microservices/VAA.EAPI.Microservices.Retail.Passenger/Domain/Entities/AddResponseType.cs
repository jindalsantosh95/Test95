namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AddResponseType
    {
        public PnrType Pnr { get; set; }
        public ErrorType Error { get; set; }
        public AddErrorListType[] AddErrorList { get; set; }
        public ResponseInfoType ResponseInfo { get; set; }
    }
}
