namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class FlightPassengerBaggage
    {
        public string FlightRefId { get; set; }
        public CheckedAndCarryOnBaggage CarryOnBag { get; set; }
        public CheckedAndCarryOnBaggage CheckedBag { get; set; }
    }
}
