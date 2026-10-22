namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class HolidayCJS
    {
        public decimal TotalPrice { get; set; }
        public decimal PricePerPerson { get; set; }
        public decimal DepositPerPerson { get; set; }
        public decimal WebDiscount { get; set; }
        public decimal Deposit { get; set; }
        public Hotel.HotelCJS? Hotel { get; set; }
        public int FlyingClubMiles { get; set; }
        public int VirginPoints { get; set; }
        public int TierPoints { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime SelectedDate { get; set; }
        public HolidayFlight? OutboundFlight { get; set; }
        public HolidayFlight? InboundFlight { get; set; }
        public IEnumerable<DirectDebit> DirectDebitSchedule { get; set; } = Enumerable.Empty<DirectDebit>();
        public bool? LocationWidened { get; set; }

    }
}
