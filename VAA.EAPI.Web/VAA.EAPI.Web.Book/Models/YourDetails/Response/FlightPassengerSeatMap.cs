namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class FlightPassengerSeatMap
    {
        public string SectorId { get; set; }
        public string SeatOfferId { get; set; }
        public string SelectedSeatOfferId { get; set; }
        public string Pos { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber { get; set; }
        public SeatPrice Price { get; set; }
    }
}
