using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    [GraphQLDescription("Parameters of the flight for the Passenger Manifest")]
    public class PassengerManifestRequest
    {
        [GraphQLDescription("Carrier code of the flight")]
        public string Carrier { get; set; }
        [GraphQLDescription("Flight Number")]
        public int FlightNumber { get; set; }
        [GraphQLDescription("Local Departure Date")]
        public string FlightDate { get; set; }
        [GraphQLDescription("Origin airport")]
        public string Origin { get; set; }
    }
}
