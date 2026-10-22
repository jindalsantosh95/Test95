using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class FareCalculation
    {
        public string PassengerType { get; set; }
        public Price Fare { get; set; }
        public IEnumerable<Taxes> Taxes { get; set; }
    }
}