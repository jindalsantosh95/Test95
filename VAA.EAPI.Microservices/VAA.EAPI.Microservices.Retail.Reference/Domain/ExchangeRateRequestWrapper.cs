using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    [XmlRoot("ExchangeRate_RQ")]
    public class ExchangeRateRequestWrapper
    {
        [XmlElement("ExchangeRate_Request")]
        public ExchangeRateRequest Request { get; set; }
    }
}
