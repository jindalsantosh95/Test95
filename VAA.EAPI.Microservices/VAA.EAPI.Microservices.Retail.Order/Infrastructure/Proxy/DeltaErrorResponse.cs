using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy
{

    [XmlRoot(ElementName = "error")]
    public class DeltaErrorResponse
    {
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [JsonProperty("developerMessage")]
        [XmlElement(ElementName = "developerMessage")]
        public string DeveloperMessage { get; set; }

        [JsonProperty("moreInfo")]
        [XmlElement(ElementName = "moreInfo")]
        public IEnumerable<Info> MoreInfo { get; set; }
    }



}
