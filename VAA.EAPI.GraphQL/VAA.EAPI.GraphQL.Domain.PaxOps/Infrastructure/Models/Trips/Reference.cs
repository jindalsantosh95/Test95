namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Reference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ConfirmationNumber { get; set; }
        public string ChainCode { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
