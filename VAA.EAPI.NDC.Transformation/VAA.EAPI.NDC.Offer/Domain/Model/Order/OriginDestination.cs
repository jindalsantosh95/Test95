using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string Distance { get; set; }
        public TimeSpan Duration { get; set; }
        public string DepartureDate { get; set; }

    }
}
