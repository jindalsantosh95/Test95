namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class TeardownContextResponseType1
    {
        public PnrType Pnr { get; set; }
        public DebugInfoType[] DebugInfo { get; set; }
        public ResponseInfoType[] ResponseInfo { get; set; }
    }
}
