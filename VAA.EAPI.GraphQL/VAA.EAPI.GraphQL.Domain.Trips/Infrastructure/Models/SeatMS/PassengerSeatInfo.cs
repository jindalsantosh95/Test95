using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class PassengerSeatInfo
    {
        public string PassengerId { get; set; }
        public string SelectedSeatIdentifier { get; set; }
        public string Ptc { get; set; }

        public static explicit operator PassengerSeatInfo((Person person, string segmentId, Basket basket) source)
        {
            var (person, segmentId, basket) = source;

            return new()
            {
                PassengerId = person.Id,
                Ptc = person.Type,
                SelectedSeatIdentifier = person.Type != "INF" ? basket.FlightPassengerSeatMap.Where(x => x.PassengerId == person.Id).First(s => s.SectorId == segmentId).SeatNumber : ""
            };
        }
    }
}