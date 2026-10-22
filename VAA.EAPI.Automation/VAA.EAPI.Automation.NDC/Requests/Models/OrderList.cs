using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.OrderList
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

    [XmlRoot(ElementName = "CreationDateTimeFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CreationDateTimeFilterCriteria
    {
        [XmlElement(ElementName = "FromDateTime", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string FromDateTime { get; set; }
        [XmlElement(ElementName = "ToDateTime", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ToDateTime { get; set; }
    }

    [XmlRoot(ElementName = "LastModifiedDateTimeFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class LastModifiedDateTimeFilterCriteria
    {
        [XmlElement(ElementName = "FromDateTime", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string FromDateTime { get; set; }
        [XmlElement(ElementName = "ToDateTime", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ToDateTime { get; set; }
    }

    [XmlRoot(ElementName = "OrderCancellationDateFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OrderCancellationDateFilterCriteria
    {
        [XmlElement(ElementName = "ExpirationDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ExpirationDate { get; set; }
    }

    [XmlRoot(ElementName = "PaymentFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaymentFilterCriteria
    {
        [XmlElement(ElementName = "PaymentStatusCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaymentStatusCode { get; set; }
    }

    [XmlRoot(ElementName = "TravelDateFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class TravelDateFilterCriteria
    {
        [XmlElement(ElementName = "EffectiveDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string EffectiveDate { get; set; }
    }

    [XmlRoot(ElementName = "OrderSearchFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OrderSearchFilterCriteria
    {
        [XmlElement(ElementName = "CreationDateTimeFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CreationDateTimeFilterCriteria CreationDateTimeFilterCriteria { get; set; }
        [XmlElement(ElementName = "LastModifiedDateTimeFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public LastModifiedDateTimeFilterCriteria LastModifiedDateTimeFilterCriteria { get; set; }
        [XmlElement(ElementName = "OrderCancellationDateFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OrderCancellationDateFilterCriteria OrderCancellationDateFilterCriteria { get; set; }
        [XmlElement(ElementName = "PaymentFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaymentFilterCriteria PaymentFilterCriteria { get; set; }
        [XmlElement(ElementName = "TravelDateFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public TravelDateFilterCriteria TravelDateFilterCriteria { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "OrderSearchFilterCriteria", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OrderSearchFilterCriteria OrderSearchFilterCriteria { get; set; }
    }

    [XmlRoot(ElementName = "IATA_OrderListRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_OrderListRQ
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