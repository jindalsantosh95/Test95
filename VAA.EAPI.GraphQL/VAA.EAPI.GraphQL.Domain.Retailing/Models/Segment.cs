using HotChocolate;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.Segment")]
    public class Segment
    {
        public static explicit operator Segment(JObject source)
        {
            return new()
            {
                SegmentID = source["SegmentID"].ToString(),
                OperatingFlight = (FlightNumber)source["OperatingFlight"],
                MarketingFlight = (FlightNumber)source["MarketingFlight"],
                Departure = source["Departure"].ToString().Parse<DateTime>(),
                Origin = source["Origin"].ToString(),
                Destination = source["Destination"].ToString(),
                Cabin = source["Cabin"].ToString(),
                Eligibility = source["Eligibility"].ToString().Parse<EligibilityType>(),
                PopsNonEligibleReason = source["PopsNonEligibleReason"].ToString()
            };
        }

        public string SegmentID { get; set; }
        public FlightNumber OperatingFlight { get; set; }
        public FlightNumber MarketingFlight { get; set; }
        public DateTime Departure { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Cabin { get; set; }
        public EligibilityType Eligibility { get; set; }
        public string PopsNonEligibleReason { get; set; }
    }
}