using VAA.EAPI.GraphQL.Domain.Trips.Models.Core;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class HolidayProductDetails : BaseProduct
    {
        public ProductSummaryDetails Summary { get; set; }
        public string BookedUser { get; set; }
        public DateTime? BookedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public RateInfo Rate { get; set; }
        public TravellerInfo TravellersComposition { get; set; }
        public HotelDetails HotelDetails { get; set; }
        public CruiseDetails CruiseDetails { get; set; }
        public CarDetails? CarDetails { get; set; }
        public FlightDetails FlightDetails { get; set; }
        public AncillaryProductDetails GenericProductDetails { get; set; }
        public SupplierDetails? SupplierDetails { get; set; }
        public bool? IsAncillaryProduct { get; set; }
        public bool? IsDerivedFromHotelSupplement { get; set; }
    }
}
