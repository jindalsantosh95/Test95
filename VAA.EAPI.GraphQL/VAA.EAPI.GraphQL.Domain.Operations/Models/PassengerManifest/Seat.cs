namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class Seat
    {
        public string SeatPosition { get; set; }
        public string Cabin { get; set; }
        public string Class { get; set; }
        public string SeatStatus { get; set; }

        public static explicit operator Seat(DTOs.PassengerManifest_SCVv7.Seat source)
        {
            if (source == null)
                return null;

            return new Seat()
            {
                SeatPosition = source.SeatPosition,
                Cabin = source.Cabin,
                Class = source.Class,
                SeatStatus = source.SeatStatus
            };
        }
    }
}
