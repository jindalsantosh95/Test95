using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AffiliationCodeType
    {

        IP = 0
    }
}
