using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("BaggageDetails", "BaggageDetails")]
    public class BaggageDetails
    {
        public IEnumerable<Route> Route { get; set; }
        public IEnumerable<CabinAllowance> CabinAllowance { get; set; }
    }
}
