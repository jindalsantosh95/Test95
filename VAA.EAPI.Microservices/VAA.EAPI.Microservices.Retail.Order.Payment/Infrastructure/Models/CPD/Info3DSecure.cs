using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Info3DSecure
    {
        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "cryptogram")]
        public Cryptogram Cryptogram { get; set; }

        [XmlElement(ElementName = "additional-data")]
        public AdditionalData Additionaldata { get; set; }
    }
}
