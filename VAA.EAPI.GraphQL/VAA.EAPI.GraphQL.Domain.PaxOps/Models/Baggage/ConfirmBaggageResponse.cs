namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class ConfirmBaggageResponse
    {
        public string OriginAirportCode { get; set; }
        public string OriginAirportName { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationAirportName { get; set; }
        public IEnumerable<PassengerBag> PassengerBags { get; set; }
        public string TotalPaidAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string DecimalPrecisionCount { get; set; }
        public string CheckInId { get; set; }

        public static explicit operator ConfirmBaggageResponse((Infrastructure.Models.BaggageMS.ConfirmBaggageResponse response, string checkInId, string travelpartyId, Basket.Basket basket) source)
        {
            return new ConfirmBaggageResponse
            {
                CheckInId = source.checkInId,
                OriginAirportCode = source.response.OriginAirportCode,
                OriginAirportName = source.response.OriginAirportName,
                DestinationAirportCode = source.response.DestinationAirportCode,
                DestinationAirportName = source.response.DestinationAirportName,
                PassengerBags = source.response.PassengerList?.Select(pax => { return (PassengerBag)(pax, source.basket); }).ToList(),
                TotalPaidAmount = source.response.TotalPaidAmount,
                DecimalPrecisionCount = source.basket.CheckedBaggageDetails.PassengerBaggage.FirstOrDefault()?.DecimalPrecisionCount.ToString(),
                CurrencyCode = source.response.CurrencyCode,
                CurrencySymbol = source.response.CurrencySymbol,
            };
        }

    }
}
