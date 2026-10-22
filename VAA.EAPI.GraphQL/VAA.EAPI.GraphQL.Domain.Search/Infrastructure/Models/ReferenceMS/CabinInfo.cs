namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS
{
    [RefDataType("cabindetails", "cabindetails")]
    public class CabinInfo
    {
        public string Carrier { get; set; }
        public string BrandName { get; set; }
        public string BrandID { get; set; }
        public string FareFamily { get; set; }
        public string? AwardFareFamily { get; set; }
    }
}
