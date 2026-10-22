using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Summary
    {
        public string Descriptor { get; set; }
        public string FareDescriptor { get; set; }
        public string UpgradeFareDescriptor { get; set; }
        public PriceWithTax Price { get; set; }
        public IEnumerable<SummaryFlight> Flights { get; set; }
        public IEnumerable<Upgrade> Upgrades { get; set; }
        public FareCalculationSummary FareCalculationSummary { get; set; }
        public IEnumerable<FareCalculation> FareCalculations { get; set; }
        public IEnumerable<Link> Links { get; set; } = Enumerable.Empty<Link>();

    }
}
