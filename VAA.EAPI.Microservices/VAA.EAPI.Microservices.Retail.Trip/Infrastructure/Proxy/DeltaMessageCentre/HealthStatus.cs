using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{

    public class HealthStatus
    {

        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {

            Pass,

            Fail,

            Warn
        }

        public StatusEnum? Status { get; set; }


    }
}
