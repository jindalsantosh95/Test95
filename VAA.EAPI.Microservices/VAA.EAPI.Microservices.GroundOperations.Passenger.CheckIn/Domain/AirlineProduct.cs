using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class AirlineProduct : Product
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public new BookingType BookingType { get => BookingType.AIRLINE; set { if (BookingType != BookingType.AIRLINE) throw new NotSupportedException(); } }
        public IEnumerable<Slice> Slices { get; set; }
    }
}