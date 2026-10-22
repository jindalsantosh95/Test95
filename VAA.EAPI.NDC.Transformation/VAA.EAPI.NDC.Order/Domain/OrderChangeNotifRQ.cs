using System.Xml.Serialization;

namespace VAA.EAPI.NDC.Order.Domain
{
    [XmlRoot(ElementName = "CCC.PNRCHNG", Namespace = "http://wse.sabre.com/eventing")]
    public class OrderChangeNotifRQ
    {
        [XmlElement(ElementName = "OWNPCC")]
        public string OWNPCC { get; set; }

        [XmlElement(ElementName = "HOMEPCC")]
        public string HOMEPCC { get; set; }

        [XmlElement(ElementName = "Locator")]
        public string Locator { get; set; }

        [XmlElement(ElementName = "EventTimeStamp")]
        public EventTimeStamp EventTimeStamp { get; set; }

        [XmlElement(ElementName = "ChangeIndicators")]
        public ChangeIndicators ChangeIndicators { get; set; }
    }
}