using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AffiliationCodeType
    {
        IP = 1,
        PL = 2
    }
}