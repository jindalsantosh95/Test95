using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Amount
    {
        [XmlAttribute(AttributeName = "alpha2code")]
        public string Alpha2code { get; set; }

        [XmlAttribute(AttributeName = "alpha3code")]
        public string Alpha3code { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }

        [XmlAttribute(AttributeName = "country-id")]
        public string CountryId { get; set; }

        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; set; }

        [XmlAttribute(AttributeName = "currency-id")]
        public string CurrencyId { get; set; }      

        [XmlAttribute(AttributeName = "format")]
        public string Format { get; set; }

        [XmlAttribute(AttributeName = "symbol")]
        public string Symbol { get; set; }

        [XmlText]
        public int Value { get; set; }
    }
}
