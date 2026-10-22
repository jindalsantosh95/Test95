using Newtonsoft.Json;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy
{
    public class Info
    {
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [JsonProperty("field")]
        [XmlElement(ElementName = "field")]
        public string Field { get; set; }
    }
}
