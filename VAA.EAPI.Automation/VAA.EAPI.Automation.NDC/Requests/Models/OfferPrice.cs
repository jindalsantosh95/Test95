using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.OfferPrice
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

    [XmlRoot(ElementName = "Pax", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Pax
    {
        [XmlElement(ElementName = "PaxID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaxID { get; set; }
        [XmlElement(ElementName = "PTC", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PTC { get; set; }
    }

    [XmlRoot(ElementName = "PaxList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaxList
    {
        [XmlElement(ElementName = "Pax", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<Pax> Pax { get; set; }
    }

    [XmlRoot(ElementName = "DataLists", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class DataLists
    {
        [XmlElement(ElementName = "PaxList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaxList PaxList { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "SelectedOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedOfferItem
    {
        [XmlElement(ElementName = "OfferItemRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OfferItemRefID { get; set; }
        [XmlElement(ElementName = "PaxRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> PaxRefID { get; set; }
    }

    [XmlRoot(ElementName = "SelectedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedOffer
    {
        [XmlElement(ElementName = "OfferRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OfferRefID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
        [XmlElement(ElementName = "SelectedOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<SelectedOfferItem> SelectedOfferItem { get; set; }
    }

    [XmlRoot(ElementName = "SelectedOfferList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedOfferList
    {
        [XmlElement(ElementName = "SelectedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SelectedOffer SelectedOffer { get; set; }
    }

    [XmlRoot(ElementName = "PricedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PricedOffer
    {
        [XmlElement(ElementName = "SelectedOfferList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SelectedOfferList SelectedOfferList { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "DataLists", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public DataLists DataLists { get; set; }
        [XmlElement(ElementName = "PricedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PricedOffer PricedOffer { get; set; }
    }

    [XmlRoot(ElementName = "IATA_OfferPriceRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_OfferPriceRQ
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