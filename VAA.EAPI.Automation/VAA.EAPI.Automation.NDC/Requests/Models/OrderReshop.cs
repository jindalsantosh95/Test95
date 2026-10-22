using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.OrderReshop
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

    [XmlRoot(ElementName = "OrderRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OrderRefID
    {
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "BookingRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class BookingRef
    {
        [XmlElement(ElementName = "BookingID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string BookingID { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PrefLevel
    {
        [XmlElement(ElementName = "PrefLevelCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PrefLevelCode { get; set; }
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
    }

    [XmlRoot(ElementName = "OriginDepCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OriginDepCriteria
    {
        [XmlElement(ElementName = "Date", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Date { get; set; }
        [XmlElement(ElementName = "IATA_LocationCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IATA_LocationCode { get; set; }
    }

    [XmlRoot(ElementName = "DestArrivalCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class DestArrivalCriteria
    {
        [XmlElement(ElementName = "IATA_LocationCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IATA_LocationCode { get; set; }
    }

    [XmlRoot(ElementName = "CabinType", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CabinType
    {
        [XmlElement(ElementName = "CabinTypeCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CabinTypeCode { get; set; }
        [XmlElement(ElementName = "PrefLevel", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PrefLevel PrefLevel { get; set; }
    }

    [XmlRoot(ElementName = "OriginDestCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OriginDestCriteria
    {
        [XmlElement(ElementName = "CabinType", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<CabinType> CabinType { get; set; }
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
    }

    [XmlRoot(ElementName = "AddOfferItems", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class AddOfferItems
    {
        [XmlElement(ElementName = "FlightRelatedCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightRelatedCriteria FlightRelatedCriteria { get; set; }
        [XmlElement(ElementName = "FlightRequest", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public FlightRequest FlightRequest { get; set; }
    }

    [XmlRoot(ElementName = "DeleteOrderItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class DeleteOrderItem
    {
        [XmlElement(ElementName = "OrderItemRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderItemRefID { get; set; }
    }

    [XmlRoot(ElementName = "ServiceOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ServiceOrder
    {
        [XmlElement(ElementName = "AddOfferItems", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public AddOfferItems AddOfferItems { get; set; }
        [XmlElement(ElementName = "DeleteOrderItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<DeleteOrderItem> DeleteOrderItem { get; set; }
    }

    [XmlRoot(ElementName = "UpdatePaxName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class UpdatePaxName
    {
        [XmlElement(ElementName = "GivenName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "PaxRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaxRefID { get; set; }
        [XmlElement(ElementName = "Surname", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Surname { get; set; }
    }

    [XmlRoot(ElementName = "ReshopOrderChoice", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ReshopOrderChoice
    {
        [XmlElement(ElementName = "ServiceOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ServiceOrder ServiceOrder { get; set; }
        [XmlElement(ElementName = "UpdatePaxName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public UpdatePaxName UpdatePaxName { get; set; }
    }

    [XmlRoot(ElementName = "ReshopOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ReshopOrder
    {
        [XmlElement(ElementName = "ReshopOrderChoice", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ReshopOrderChoice ReshopOrderChoice { get; set; }
    }

    [XmlRoot(ElementName = "CancelOrderRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CancelOrderRef
    {
        [XmlElement(ElementName = "OrderRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderRefID { get; set; }
    }

    [XmlRoot(ElementName = "UpdateOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class UpdateOrder
    {
        [XmlElement(ElementName = "ReshopOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ReshopOrder ReshopOrder { get; set; }
        [XmlElement(ElementName = "CancelOrderRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CancelOrderRef CancelOrderRef { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "BookingRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public BookingRef BookingRef { get; set; }
        [XmlElement(ElementName = "OrderRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderRefID { get; set; }
        [XmlElement(ElementName = "UpdateOrder", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public UpdateOrder UpdateOrder { get; set; }
    }

    [XmlRoot(ElementName = "IATA_OrderReshopRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_OrderReshopRQ
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
