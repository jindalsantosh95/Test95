using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Product
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public BookingType BookingType { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public string Status { get; set; }
    }
}