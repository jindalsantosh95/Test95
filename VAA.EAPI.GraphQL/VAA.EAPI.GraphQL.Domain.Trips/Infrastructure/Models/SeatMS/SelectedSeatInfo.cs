namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class SelectedSeatInfo
    {
        public string AssociationId { get; set; }
        public string OfferId { get; set; }
        public string OfferItemNumber { get; set; }
        public string FlightIdRef { get; set; }
        public string SeatNumber { get; set; }
        public string CustomerRefId { get; set; }
        public CurrencyAmount Amount { get; set; }
    }
}