namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class ConfirmBaggageResponse
    {
        public string OriginAirportCode { get; set; }
        public string OriginAirportName { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationAirportName { get; set; }
        public IEnumerable<Passenger> PassengerList { get; set; }
        public string TotalPaidAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string DecimalPrecisionCount { get; set; }
    }
}
