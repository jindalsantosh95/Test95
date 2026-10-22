namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    public class Reference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ConfirmationNumber { get; set; }
        public string ChainCode { get; set; }
        public DateTime BookingDate { get; set; }
        public string OrderId { get; set; }

    }
}
