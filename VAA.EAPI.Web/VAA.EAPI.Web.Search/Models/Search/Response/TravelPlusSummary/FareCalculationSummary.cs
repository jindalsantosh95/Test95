using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class FareCalculationSummary
    {
        public IEnumerable<Fare> Fares { get; set; }
        public IEnumerable<Taxes> Taxes { get; set; }
    }
}