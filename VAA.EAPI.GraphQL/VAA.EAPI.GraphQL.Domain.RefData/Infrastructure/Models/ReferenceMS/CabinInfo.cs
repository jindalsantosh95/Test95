namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("cabindetails", "cabindetails")]
    public class CabinInfo
    {
        public string Carrier { get; set; }
        public string BrandName { get; set; }
        public string BrandID { get; set; }
        public string FareFamily { get; set; }
    }
}
