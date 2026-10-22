using VAA.EAPI.GraphQL.Domain.Search.Infrastructure;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Reference
{
    [RefDataType("BaggageDetails", "BaggageDetails")]
    public class BaggageDetails
    {
        public IEnumerable<Route>? Route { get; set; }
        public IEnumerable<CabinAllowance>? CabinAllowance { get; set; }
    }
}
