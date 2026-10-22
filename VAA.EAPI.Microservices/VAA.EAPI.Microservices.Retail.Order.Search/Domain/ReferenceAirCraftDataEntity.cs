using Microsoft.Azure.Cosmos.Table;
namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class ReferenceAirCraftDataEntity : TableEntity
    {
        public string AircraftCode { get; set; }
        public string AircraftName { get; set; }

    }
}
