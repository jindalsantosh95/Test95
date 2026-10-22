using Microsoft.Azure.Cosmos.Table;
namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class PieceAllowanceTableEntity : TableEntity
    {
        public string DimensionCategory { get; set; }
        public string UnitCode { get; set; }
        public string MaxMeasure { get; set; }

    }
}
