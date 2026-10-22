namespace VAA.EAPI.GraphQL.Domain.Book.Models.Offer
{
    public class SeatInformation
    {
        public string SeatColumn { get; set; }
        public int SeatRowNumber { get; set; }
        public string FlightLegId { get; set; }
        public string SeatProductCode { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitRow { get; set; }
        public bool ExitOverride { get; set; }
    }
}