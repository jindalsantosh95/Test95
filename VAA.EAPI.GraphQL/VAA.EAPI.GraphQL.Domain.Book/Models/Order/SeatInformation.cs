namespace VAA.EAPI.GraphQL.Domain.Book.Models.Order
{
    public class SeatInformation
    {
        public string FlightLegId { get; set; }
        public string SeatColumnName { get; set; }
        public string SeatRowNum { get; set; }
        public string SeatProductCode { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitRow { get; set; }
        public bool ExitOverride { get; set; }
        public string SeatStatus { get; set; }
    }
}