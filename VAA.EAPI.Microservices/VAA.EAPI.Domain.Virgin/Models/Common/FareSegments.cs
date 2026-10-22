namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class FareSegments
    {
        public string CabinName { get; set; }
        public string BookingClass { get; set; }
        public bool IsDominantLeg { get; set; }
        public string FlightSegmentRefID { get; set; }
        public string FareBasisCode { get; set; }
        public string TicketDesignator { get; set; }
        public bool? IsSaverFare { get; set; }
        public Price BaselineCabinSalePrice { get; set; }
        public Price CurrentCabinSalePrice { get; set; }

    }
}
