using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class ExchangeRateRequest
    {
        [XmlAttribute]
        public string callingApplication { get; set; }

        [XmlAttribute]
        public string transactionID { get; set; }

        [XmlAttribute]
        public string valueToConvert { get; set; }

        [XmlAttribute]
        public string originalCurrencyISOCode { get; set; }

        [XmlAttribute]
        public string originalCurrency { get; set; }

        [XmlAttribute]
        public string targetCurrencyISOCode { get; set; }

        [XmlAttribute]
        public string targetCurrency { get; set; }

        [XmlAttribute]
        public string exchangeRateType { get; set; }

        [XmlAttribute]
        public string exchangeRateDate { get; set; }
    }

}
