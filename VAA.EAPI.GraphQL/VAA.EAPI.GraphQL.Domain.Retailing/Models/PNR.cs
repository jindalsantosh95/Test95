using HotChocolate;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.PNR")]
    public class PNR
    {
        public static explicit operator PNR(JObject source)
        {
            return new()
            {
                RecordLocator = source["RecordLocator"].ToString(),
                BookingDate = source["BookingDate"].ToString().Parse<DateTime>(),
                Segments = source["Segments"].ToArray().Select(x => (Segment)x),
                Passengers = source["Passengers"].ToArray().Select(x => (Passenger)x)
            };
        }

        public string RecordLocator { get; set; }
        public DateTime BookingDate { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
    }
}
