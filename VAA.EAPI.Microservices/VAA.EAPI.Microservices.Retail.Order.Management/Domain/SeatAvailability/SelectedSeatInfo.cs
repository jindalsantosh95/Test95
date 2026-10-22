namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability
{
    public class SelectedSeatInfo
    {
        public string AssociationId { get; set; }
        public string OfferId { get; set; }
        public string OfferItemNumber { get; set; }
        public string FlightIdRef { get; set; }
        public string SeatNumber { get; set; }
    }
}
