namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class ProductSummaryDetails
    {
        public string AvailabilityStatus { get; set; }
        public string ProductStatus { get; set; }
        public string ProductSubStatus { get; set; }
        public string ProductType { get; set; }
        public string Name { get; set; }
        public string Highlight { get; set; }
        public string PrimaryAttribute { get; set; }
        public string SecondaryAttribute { get; set; }
        public string ServiceStartDate { get; set; }
        public string ServiceEndDate { get; set; }

        public string DurationDescription { get; set; }
        public TravelDatesInfo TravelDates { get; set; }
        public TravelDurationInfo TravelDuration { get; set; }
        public bool Recommended { get; set; }
        public bool SpecialOffer { get; set; }
        public string ItemSource { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal? PriceBeforeDiscount { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string QuoteExpiryDate { get; set; }
        public string ExperienceType { get; set; }
        public TravelTimes TravelTimings { get; set; }
        public decimal? TotalAmendCharges { get; set; }
        public decimal? TotalCancellationCharges { get; set; }
        public bool? VipBooking { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? BookedDate { get; set; }
        public string? BookingType { get; set; }
        public string? BookingTypeName { get; set; }
        public string? BrandName { get; set; }
        public string? LeadPassengerName { get; set; }
        public string? ElementGroup { get; set; }
        public string? ElementGroupName { get; set; }

    }
}