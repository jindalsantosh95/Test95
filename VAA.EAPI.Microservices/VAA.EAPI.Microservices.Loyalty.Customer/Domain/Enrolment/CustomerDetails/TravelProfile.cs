using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    public class TravelProfile
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ReasonsForTravel
        {
            [EnumMember(Value = "Business")]
            Business = 0,
            [EnumMember(Value = "Leisure")]
            Leisure = 1,
            [EnumMember(Value = "Both")]
            Both = 2
        }
        public ReasonsForTravel? ReasonForTravel { get; set; }
    }
}
