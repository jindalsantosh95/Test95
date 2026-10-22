namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class ShoppingCart
    {
        public string CostOfBasket { get; set; }
        public string OutstandingBalanceWithBasket { get; set; }
        public string CostOfBookingWithBasket { get; set; }
        public List<Selection> AttractionTickets { get; set; }
        public List<Selection> Excursions { get; set; }
        public List<Selection> Vrooms { get; set; }
        public List<Selection> DepartureBeaches { get; set; }
        public List<Selection> CubaVisas { get; set; }
        public List<Selection> All { get; set; }
    }
}

