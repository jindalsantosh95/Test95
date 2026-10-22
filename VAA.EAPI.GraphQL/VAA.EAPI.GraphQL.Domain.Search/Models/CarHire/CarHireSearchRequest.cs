namespace VAA.EAPI.GraphQL.Domain.Search.Models.CarHire
{
    public class CarHireSearchRequest
    {
        public string? BookingType { get; set; }
        public string? PickUpDepot { get; set; }
        public string? DropOffDepot { get; set; }
        public string? DepartureDate { get; set; }
        public string? Duration { get; set; }
        public string? PickUpTime { get; set; }
        public string? DropOffTime { get; set; }
    }


}
