using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class CabinBaggageEntity : TableEntity
    {
        public string DimensionCategory { get; set; }
        public string UnitCode { get; set; }
        public string Value { get; set; }
        public string TotalQuantity { get; set; }

        public string AllowanceType { get; set; }



    }
}
