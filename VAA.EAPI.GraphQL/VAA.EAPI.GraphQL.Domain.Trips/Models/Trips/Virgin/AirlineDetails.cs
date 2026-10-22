namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class AirlineDetails
    {
        public string? FlightNumber { get; set; }
        public string? CarrierCode { get; set; }
        public string? Carriername { get; set; }

        public static explicit operator AirlineDetails(Infrastructure.Models.TripMS.AirlineDetails source)
        {
            if (source == null) return null;

            return new AirlineDetails()
            {
                CarrierCode = source.CarrierCode,
                Carriername = source.Carriername,
                FlightNumber = source.FlightNumber
            };
        }
    }
}
