using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.AirShopping
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

    [XmlRoot(ElementName = "FlightCharacteristicsCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FlightCharacteristicsCriteria
    {
        [XmlElement(ElementName = "CharacteristicCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CharacteristicCode { get; set; }
        [XmlElement(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PrefLevel PrefLevel { get; set; }
    }

    [XmlRoot(ElementName = "FlightCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FlightCriteria
    {
        [XmlElement(ElementName = "FlightCharacteristicsCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightCharacteristicsCriteria FlightCharacteristicsCriteria { get; set; }
    }

    [XmlRoot(ElementName = "FlightRelatedCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FlightRelatedCriteria
    {
        [XmlElement(ElementName = "FlightCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightCriteria FlightCriteria { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PrefLevel
    {
        [XmlElement(ElementName = "PrefLevelCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PrefLevelCode { get; set; }
    }

    [XmlRoot(ElementName = "CabinType", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CabinType
    {
        [XmlElement(ElementName = "CabinTypeCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CabinTypeCode { get; set; }
        [XmlElement(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PrefLevel PrefLevel { get; set; }
    }

    [XmlRoot(ElementName = "DestArrivalCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class DestArrivalCriteria
    {
        [XmlElement(ElementName = "IATA_LocationCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IATA_LocationCode { get; set; }
    }

    [XmlRoot(ElementName = "OriginDepCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OriginDepCriteria
    {
        [XmlElement(ElementName = "Date", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Date { get; set; }
        [XmlElement(ElementName = "IATA_LocationCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IATA_LocationCode { get; set; }
    }
    [XmlRoot(ElementName = "CalendarDateCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CalendarDateCriteria
    {

        [XmlElement(ElementName = "DaysAfterNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public int DaysAfterNumber { get; set; }

        [XmlElement(ElementName = "DaysBeforeNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public int DaysBeforeNumber { get; set; }
    }
    [XmlRoot(ElementName = "OriginDestCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OriginDestCriteria
    {
        [XmlElement(ElementName = "CabinType", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CabinType CabinType { get; set; }
        [XmlElement(ElementName = "CalendarDateCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CalendarDateCriteria CalendarDateCriteria { get; set; }
        [XmlElement(ElementName = "DestArrivalCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public DestArrivalCriteria DestArrivalCriteria { get; set; }
        [XmlElement(ElementName = "OriginDepCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OriginDepCriteria OriginDepCriteria { get; set; }
    }

    [XmlRoot(ElementName = "FlightRequestOriginDestinationsCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FlightRequestOriginDestinationsCriteria
    {
        [XmlElement(ElementName = "OriginDestCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<OriginDestCriteria> OriginDestCriteria { get; set; }
    }

    [XmlRoot(ElementName = "FlightRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FlightRequest
    {
        [XmlElement(ElementName = "FlightRequestOriginDestinationsCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightRequestOriginDestinationsCriteria FlightRequestOriginDestinationsCriteria { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
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
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "FareCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class FareCriteria
    {
        [XmlElement(ElementName = "FareSubTypeCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string FareSubTypeCode { get; set; }

        [XmlElement(ElementName = "FareTypeCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string FareTypeCode { get; set; }
        [XmlElement(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PrefLevel PrefLevel { get; set; }
    }

    [XmlRoot(ElementName = "ServiceCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ServiceCriteria
    {
        [XmlElement(ElementName = "IncludeInd", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IncludeInd { get; set; }
        [XmlElement(ElementName = "TaxonomyCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> TaxonomyCode { get; set; }
    }

    [XmlRoot(ElementName = "Org", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Org
    {
        [XmlElement(ElementName = "Name", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Name { get; set; }
        [XmlElement(ElementName = "OrgID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrgID { get; set; }
    }

    [XmlRoot(ElementName = "ProgramOwner", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ProgramOwner
    {
        [XmlElement(ElementName = "Org", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Org Org { get; set; }
    }

    [XmlRoot(ElementName = "ProgramCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ProgramCriteria
    {
        [XmlElement(ElementName = "ProgramOwner", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ProgramOwner ProgramOwner { get; set; }
    }

    [XmlRoot(ElementName = "OfferCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OfferCriteria
    {
        [XmlElement(ElementName = "FareCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FareCriteria FareCriteria { get; set; }
        [XmlElement(ElementName = "ServiceCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ServiceCriteria ServiceCriteria { get; set; }
        [XmlElement(ElementName = "ProgramCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ProgramCriteria ProgramCriteria { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }


    [XmlRoot(ElementName = "CurParameter", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CurParameter
    {
        [XmlElement(ElementName = "CurCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CurCode { get; set; }
    }

    [XmlRoot(ElementName = "ResponseParameters", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ResponseParameters
    {
        [XmlElement(ElementName = "CurParameter", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CurParameter CurParameter { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "FlightRelatedCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightRelatedCriteria FlightRelatedCriteria { get; set; }
        [XmlElement(ElementName = "FlightRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightRequest FlightRequest { get; set; }
        [XmlElement(ElementName = "OfferCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OfferCriteria OfferCriteria { get; set; }
        [XmlElement(ElementName = "PaxList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaxList PaxList { get; set; }
        [XmlElement(ElementName = "ResponseParameters", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ResponseParameters ResponseParameters { get; set; }

    }

    [XmlRoot(ElementName = "IATA_AirShoppingRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_AirShoppingRQ
    {
        [XmlElement(ElementName = "DistributionChain", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public DistributionChain DistributionChain { get; set; }
        [XmlElement(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public Request Request { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}