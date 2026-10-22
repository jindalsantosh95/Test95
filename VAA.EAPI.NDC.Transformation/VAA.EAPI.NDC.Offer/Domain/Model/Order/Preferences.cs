using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Preferences
    {
        public string PreferredAirline { get; set; }
        public IEnumerable<string> ClassPreferences { get; set; }
        public IEnumerable<string> AirlinePreferences { get; set; }
        public IEnumerable<string> FlightPreferences { get; set; }
        public IEnumerable<string> FightServiceProductPreferences { get; set; }
        public int? NumberStops { get; set; }
        public string SeatPreferences { get; set; }
        public decimal? FlightDuration { get; set; }
        public decimal? FlightSegmentDuration { get; set; }
        public decimal? MCT { get; set; }
        public CurrencyAmount TotalPrice { get; set; }


    }
}
