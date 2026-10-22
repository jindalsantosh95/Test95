using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.ServiceList
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

    [XmlRoot(ElementName = "Offer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Offer
    {
        [XmlElement(ElementName = "OfferID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OfferID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
    }

    [XmlRoot(ElementName = "OfferRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OfferRequest
    {
        [XmlElement(ElementName = "Offer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Offer Offer { get; set; }
    }

    [XmlRoot(ElementName = "Order", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Order
    {
        [XmlElement(ElementName = "OrderID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
    }

    [XmlRoot(ElementName = "OrderRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OrderRequest
    {
        [XmlElement(ElementName = "Order", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Order Order { get; set; }
    }

    [XmlRoot(ElementName = "ServiceListCoreRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ServiceListCoreRequest
    {

        [XmlElement(ElementName = "OfferRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OfferRequest OfferRequest { get; set; }


        [XmlElement(ElementName = "OrderRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OrderRequest OrderRequest { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "ServiceListCoreRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ServiceListCoreRequest ServiceListCoreRequest { get; set; }
    }

    [XmlRoot(ElementName = "IATA_ServiceListRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_ServiceListRQ
    {
        [XmlElement(ElementName = "DistributionChain", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public DistributionChain DistributionChain { get; set; }
        [XmlElement(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public Request Request { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
