using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy
{
    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Root));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Root)serializer.Deserialize(reader);
    // }
    [XmlRoot(ElementName = "voidTransactionRequest")]
    public class VoidTransactionRequest
    {

        [XmlElement(ElementName = "refund")]
        public Refund Refund { get; set; }
    }
    [XmlRoot(ElementName = "amount")]
    public class Amount
    {

        [XmlAttribute(AttributeName = "country-id")]
        public int CountryId { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "transaction")]
    public class Transaction
    {

        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }

        [XmlAttribute(AttributeName = "order-no")]
        public string OrderNo { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "refund")]
    public class Refund
    {

        [XmlElement(ElementName = "transaction")]
        public Transaction Transaction { get; set; }

        [XmlAttribute(AttributeName = "client-id")]
        public string ClientId { get; set; }

        [XmlText]
        public int Text { get; set; }
    }



    //	Sample
    //-----

    //<? xml version="1.0" encoding="UTF-8"?>
    // <root> <refund client-id = "[integer]">
    // <transaction order-no="XCDFGG" id="1859798">
    // <amount country-id="200">14000</amount>
    // </transaction> 
    // </refund>
    //</root>

}
