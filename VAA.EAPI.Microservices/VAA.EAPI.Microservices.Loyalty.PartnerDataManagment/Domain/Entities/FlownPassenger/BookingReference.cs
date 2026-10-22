
namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookingChannel { get; set; }
    }
}
