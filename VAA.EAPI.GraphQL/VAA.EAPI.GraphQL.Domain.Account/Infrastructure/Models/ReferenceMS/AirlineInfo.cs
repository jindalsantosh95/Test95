namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS
{
    [RefDataType("airlinedetails", "airlinedetails")]
    public class AirlineInfo
    {
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }
    }
}
