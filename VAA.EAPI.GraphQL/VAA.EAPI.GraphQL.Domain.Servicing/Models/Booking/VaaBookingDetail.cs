using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class VaaBookingDetail
    {
        [GraphQLDescription("Document OrderId")]
        public string? OrderId { get; set; }
        [GraphQLDescription("Old/New Booking Flag")]
        public string? IsOldBooking { get; set; }
        [GraphQLDescription("Fare Price Details")]
        public FarePrice? FarePrice { get; set; }
        [GraphQLDescription("Fees&Taxes Details")]
        public TotalFeesAndTaxes? TotalFeesAndTaxes { get; set; }
        [GraphQLDescription("Booking Creation time")]
        public VaaBookingCreationDateTime? VaaBookingCreationDateTime { get; set; }
        [GraphQLDescription("Record Locator")]
        public string? Rloc { get; set; }
        [GraphQLDescription("Record locator creation date")]
        public string? RlocCreateDate { get; set; }
        [GraphQLDescription("POS Details")]
        public OriginalPos? OriginalPos { get; set; }
        [GraphQLDescription("Booking segment details")]
        public List<VaaBookingSegmentDetail>? VaaBookingSegmentDetails { get; set; }
    }
}
