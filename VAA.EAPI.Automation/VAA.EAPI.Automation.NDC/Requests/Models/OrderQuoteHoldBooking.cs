using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.OrderQuoteHoldBooking
{
    [XmlRoot(ElementName = "ParticipatingOrg", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ParticipatingOrg
    {
        [XmlElement(ElementName = "Name", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Name { get; set; }
        [XmlElement(ElementName = "OrgID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrgID { get; set; }
    }

    [XmlRoot(ElementName = "DistributionChainLink", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class DistributionChainLink
    {
        [XmlElement(ElementName = "Ordinal", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Ordinal { get; set; }
        [XmlElement(ElementName = "OrgRole", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrgRole { get; set; }
        [XmlElement(ElementName = "ParticipatingOrg", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ParticipatingOrg ParticipatingOrg { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "DistributionChain", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class DistributionChain
    {
        [XmlElement(ElementName = "DistributionChainLink", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<DistributionChainLink> DistributionChainLink { get; set; }
    }

    [XmlRoot(ElementName = "RepricedOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class RepricedOrder
    {
        [XmlElement(ElementName = "OrderID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Payload", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Payload
    {
        [XmlElement(ElementName = "RepricedOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public RepricedOrder RepricedOrder { get; set; }
    }

    [XmlRoot(ElementName = "IATA_OrderQuoteRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_OrderQuoteRQ
    {
        [XmlElement(ElementName = "DistributionChain", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public DistributionChain DistributionChain { get; set; }
        [XmlElement(ElementName = "Payload", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public Payload Payload { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }

}
