namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AddPhoneResponseType
    {
        public PnrType Pnr { get; set; }
        public ErrorType Error { get; set; }
        public AddPhoneErrorListType[] AddPhoneErrorList { get; set; }
        public ResponseInfoType ResponseInfo { get; set; }
    }
}
