namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class PartnerBookingClass
    {
        public string RedemptionClassIndicator { get; set; }
        public string BookingClass { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CabinClass { get; set; }
        public string PartnerCode { get; set; }
    }
}
