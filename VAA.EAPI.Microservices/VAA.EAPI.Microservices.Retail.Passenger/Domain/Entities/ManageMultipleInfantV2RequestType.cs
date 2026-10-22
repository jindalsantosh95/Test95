namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class ManageMultipleInfantV2RequestType
    {
        public RequestInfoType RequestInfo { get; set; }
        public InfantV2InfoType[] InfantV2Info { get; set; }
    }
}
