using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary
{
    public class Summary
    {
        public Price Price { get; set; }
        public IEnumerable<Flight> Flights { get; set; }
        public IEnumerable<Upgrade> Upgrades { get; set; }
        public FareCalculationSummary FareCalculationSummary { get; set; }
        public IEnumerable<FareCalculation> FareCalculations { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public string Id { get; set; }
        public string FareId { get; set; }
        public string? BasketId { get; set; }
        public bool? IsSaverFare { get; set; }
        public string UpgradeFareId { get; set; }
        public IEnumerable<FlightPassengerBaggage> FlightPassengerBaggage { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string PromoCodeApplied { get; set; }
        public EarnPoints? EarnPoints { get; set; }
        public decimal? PriceChange { get; set; }
    }
}