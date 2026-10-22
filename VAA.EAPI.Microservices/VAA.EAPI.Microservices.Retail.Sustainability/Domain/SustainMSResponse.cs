namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public class SustainMSResponse
    {
        public static explicit operator SustainMSResponse(CarbonFootprint source)
        {
            if (source == null) return null;

            return new()
            {
                OriginAirport = source.Origin,
                DestinationAirport = source.Destination,
                FlightDate = source.Date,
                FlightNumber = source.FlightNumber,
                KilosCo2 = source.KilosCo2,
                KilosCo2e = source.KilosCo2e,
                Source = source.Source,
                ErrorMessage = source.ErrorMessage,
                LbsCo2 = source.LbsCo2,
                SourceMethodology = source.SourceMethodology,
                SourceQuality = source.SourceQuality,
                LbsCo2e = source.LbsCo2e,
                RequestId = source.RequestId,
                StatusCode = source.StatusCode,
                Cabin = source.TravelClass
            };
        }

        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNumber { get; set; }
        public string FlightDate { get; set; }
        public string RequestId { get; set; }
        public string SourceMethodology { get; set; }
        public string SourceQuality { get; set; }
        public decimal KilosCo2 { get; set; }
        public decimal KilosCo2e { get; set; }
        public decimal LbsCo2 { get; set; }
        public decimal LbsCo2e { get; set; }
        public string Source { get; set; }
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Cabin { get; set; }
    }
}
