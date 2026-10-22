namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class TransactionMutationFlightDetails
    {
        public string BookingClass { get; set; }
        public string BookingReference { get; set; }
        public string OriginationAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string TicketNumber { get; set; }
    }
}
