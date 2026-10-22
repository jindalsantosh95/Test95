using System.Xml.Serialization;

namespace VAA.EAPI.Automation.NDC.Requests.Models.OrderChangeService
{
    [XmlRoot(ElementName = "Phone", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Phone
    {
        [XmlElement(ElementName = "ContactTypeText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ContactTypeText { get; set; }
        [XmlElement(ElementName = "CountryDialingCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryDialingCode { get; set; }
        [XmlElement(ElementName = "PhoneNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PhoneNumber { get; set; }
    }
    [XmlRoot(ElementName = "EmailAddress", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class EmailAddress
    {
        [XmlElement(ElementName = "EmailAddressText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string EmailAddressText { get; set; }
    }

    [XmlRoot(ElementName = "PostalAddress", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PostalAddress
    {

        [XmlElement(ElementName = "CityName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CityName { get; set; }

        [XmlElement(ElementName = "CountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryCode { get; set; }

        [XmlElement(ElementName = "CountrySubDivisionName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountrySubDivisionName { get; set; }

        [XmlElement(ElementName = "PostalCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public int PostalCode { get; set; }

        [XmlElement(ElementName = "StreetText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string StreetText { get; set; }
    }

    [XmlRoot(ElementName = "ContactInfo", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ContactInfo
    {
        [XmlElement(ElementName = "ContactInfoID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ContactInfoID { get; set; }
        [XmlElement(ElementName = "EmailAddress", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public EmailAddress EmailAddress { get; set; }
        [XmlElement(ElementName = "Phone", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Phone Phone { get; set; }
        [XmlElement(ElementName = "PostalAddress")]
        public PostalAddress PostalAddress { get; set; }
    }
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
        [XmlElement(ElementName = "ContactInfo", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ContactInfo ContactInfo { get; set; }
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

    [XmlRoot(ElementName = "BookingRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class BookingRef
    {
        [XmlElement(ElementName = "BookingID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string BookingID { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "SelectedSeat", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedSeat
    {
        [XmlElement(ElementName = "ColumnID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ColumnID { get; set; }
        [XmlElement(ElementName = "SeatRowNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string SeatRowNumber { get; set; }
    }

    [XmlRoot(ElementName = "PaxSegmentReferences", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaxSegmentReferences
    {
        [XmlElement(ElementName = "PaxSegmentRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> PaxSegmentRefID { get; set; }
    }

    [XmlRoot(ElementName = "OfferFlightAssociations", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class OfferFlightAssociations
    {
        [XmlElement(ElementName = "PaxSegmentReferences", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaxSegmentReferences PaxSegmentReferences { get; set; }
    }

    [XmlRoot(ElementName = "SelectedALaCarteOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedALaCarteOfferItem
    {
        [XmlElement(ElementName = "OfferFlightAssociations", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public OfferFlightAssociations OfferFlightAssociations { get; set; }
        [XmlElement(ElementName = "Qty", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Qty { get; set; }
    }
    
    [XmlRoot(ElementName = "SelectedOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedOfferItem
    {
        [XmlElement(ElementName = "OfferItemRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OfferItemRefID { get; set; }
        [XmlElement(ElementName = "PaxRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> PaxRefID { get; set; }
        [XmlElement(ElementName = "SelectedSeat", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SelectedSeat SelectedSeat { get; set; }
        [XmlElement(ElementName = "SelectedALaCarteOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SelectedALaCarteOfferItem SelectedALaCarteOfferItem { get; set; }
    }

    [XmlRoot(ElementName = "TotalOfferPriceAmount", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class TotalOfferPriceAmount
    {
        [XmlAttribute(AttributeName = "CurCode")]
        public string CurCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SelectedPricedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SelectedPricedOffer
    {
        [XmlElement(ElementName = "OfferRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OfferRefID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
        [XmlElement(ElementName = "TotalOfferPriceAmount", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public TotalOfferPriceAmount TotalOfferPriceAmount { get; set; }

        [XmlElement(ElementName = "SelectedOfferItem", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<SelectedOfferItem> SelectedOfferItem { get; set; }
    }

    [XmlRoot(ElementName = "AcceptSelectedQuotedOfferList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class AcceptSelectedQuotedOfferList
    {
        [XmlElement(ElementName = "SelectedPricedOffer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<SelectedPricedOffer> SelectedPricedOffer { get; set; }
    }

    [XmlRoot(ElementName = "New", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class New
    {
        [XmlElement(ElementName = "PaxRefID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaxRefID { get; set; }
    }

    [XmlRoot(ElementName = "UpdatePax", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class UpdatePax
    {
        [XmlElement(ElementName = "New", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<New> New { get; set; }
    }

    [XmlRoot(ElementName = "ChangeOrderChoice", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class ChangeOrderChoice
    {
        [XmlElement(ElementName = "AcceptSelectedQuotedOfferList", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public AcceptSelectedQuotedOfferList AcceptSelectedQuotedOfferList { get; set; }
        [XmlElement(ElementName = "UpdatePax", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public UpdatePax UpdatePax { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "SalesTaxRegistration", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SalesTaxRegistration
    {
        [XmlElement(ElementName = "CityName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CityName { get; set; }
        [XmlElement(ElementName = "CountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryCode { get; set; }
        [XmlElement(ElementName = "CountryDialingCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryDialingCode { get; set; }
        [XmlElement(ElementName = "EmailAddressText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string EmailAddressText { get; set; }
        [XmlElement(ElementName = "PhoneNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PhoneNumber { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PostalCode { get; set; }
        [XmlElement(ElementName = "SalesTaxRegistrationID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string SalesTaxRegistrationID { get; set; }
        [XmlElement(ElementName = "StreetText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> StreetText { get; set; }
    }


    [XmlRoot(ElementName = "Employer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Employer
    {
        [XmlElement(ElementName = "Name", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Name { get; set; }
        [XmlElement(ElementName = "SalesTaxRegistration", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SalesTaxRegistration SalesTaxRegistration { get; set; }
        [XmlElement(ElementName = "TaxDeductedatSourceCapabilityInd", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string TaxDeductedatSourceCapabilityInd { get; set; }
    }


    [XmlRoot(ElementName = "Visa", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Visa
    {
        [XmlElement(ElementName = "ExpiryDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ExpiryDate { get; set; }
        [XmlElement(ElementName = "HostCountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string HostCountryCode { get; set; }
        [XmlElement(ElementName = "IssuingCountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IssuingCountryCode { get; set; }
        [XmlElement(ElementName = "VisaID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string VisaID { get; set; }
    }

    [XmlRoot(ElementName = "IdentityDoc", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class IdentityDoc
    {
        [XmlElement(ElementName = "CitizenshipCountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CitizenshipCountryCode { get; set; }
        [XmlElement(ElementName = "ExpiryDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ExpiryDate { get; set; }
        [XmlElement(ElementName = "GivenName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "IdentityDocID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IdentityDocID { get; set; }
        [XmlElement(ElementName = "IdentityDocTypeCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IdentityDocTypeCode { get; set; }
        [XmlElement(ElementName = "IssueDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IssueDate { get; set; }
        [XmlElement(ElementName = "IssuingCountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string IssuingCountryCode { get; set; }
        [XmlElement(ElementName = "ResidenceCountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ResidenceCountryCode { get; set; }
        [XmlElement(ElementName = "Surname", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Surname { get; set; }
        [XmlElement(ElementName = "Visa", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Visa Visa { get; set; }
    }

    [XmlRoot(ElementName = "RedressCase", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class RedressCase
    {
        [XmlElement(ElementName = "CountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryCode { get; set; }
        [XmlElement(ElementName = "RedressCaseID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string RedressCaseID { get; set; }
    }


    [XmlRoot(ElementName = "Pax", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Pax
    {
        [XmlElement(ElementName = "PaxID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaxID { get; set; }
        [XmlElement(ElementName = "PTC", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PTC { get; set; }
        [XmlElement(ElementName = "IdentityDoc", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<IdentityDoc> IdentityDoc { get; set; }

        [XmlElement(ElementName = "Employer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Employer Employer { get; set; }
        [XmlElement(ElementName = "RedressCase", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public RedressCase RedressCase { get; set; }

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

    [XmlRoot(ElementName = "Order", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Order
    {
        [XmlElement(ElementName = "OrderID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OrderID { get; set; }
        [XmlElement(ElementName = "OwnerCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string OwnerCode { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Amount", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Amount
    {
        [XmlAttribute(AttributeName = "CurCode")]
        public string CurCode { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "IndividualName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class IndividualName
    {
        [XmlElement(ElementName = "GivenName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string GivenName { get; set; }
        [XmlElement(ElementName = "Surname", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string Surname { get; set; }
    }

    [XmlRoot(ElementName = "PayerName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PayerName
    {
        [XmlElement(ElementName = "IndividualName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public IndividualName IndividualName { get; set; }
    }

    [XmlRoot(ElementName = "Payer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class Payer
    {
        [XmlElement(ElementName = "PayerName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PayerName PayerName { get; set; }
    }

    [XmlRoot(ElementName = "CardholderAddress", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class CardholderAddress
    {
        [XmlElement(ElementName = "CityName", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CityName { get; set; }
        [XmlElement(ElementName = "CountryCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CountryCode { get; set; }
        [XmlElement(ElementName = "PostalCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PostalCode { get; set; }
        [XmlElement(ElementName = "StreetText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public List<string> StreetText { get; set; }
    }

    [XmlRoot(ElementName = "SecurePaymentVersion2", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SecurePaymentVersion2
    {
        [XmlElement(ElementName = "AuthenticationValue", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string AuthenticationValue { get; set; }
        [XmlElement(ElementName = "DirectoryServerTrxID", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string DirectoryServerTrxID { get; set; }
        [XmlElement(ElementName = "ElectronicCommerceInd", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ElectronicCommerceInd { get; set; }
        [XmlElement(ElementName = "TrxStatusText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string TrxStatusText { get; set; }
        [XmlElement(ElementName = "PaymentTrxChannelCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string PaymentTrxChannelCode { get; set; }
    }

    [XmlRoot(ElementName = "SecureProgram", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class SecureProgram
    {
        [XmlElement(ElementName = "EnrollmentStatusText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string EnrollmentStatusText { get; set; }
    }

    [XmlRoot(ElementName = "PaymentCard", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaymentCard
    {
        [XmlElement(ElementName = "CardholderAddress", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public CardholderAddress CardholderAddress { get; set; }
        [XmlElement(ElementName = "CardNumber", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CardNumber { get; set; }
        [XmlElement(ElementName = "CardSecurityCode", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CardSecurityCode { get; set; }
        [XmlElement(ElementName = "CardTypeText", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string CardTypeText { get; set; }
        [XmlElement(ElementName = "ExpirationDate", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public string ExpirationDate { get; set; }
        [XmlElement(ElementName = "SecurePaymentVersion2", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SecurePaymentVersion2 SecurePaymentVersion2 { get; set; }
        [XmlElement(ElementName = "SecureProgram", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public SecureProgram SecureProgram { get; set; }
    }

    [XmlRoot(ElementName = "PaymentMethod", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaymentMethod
    {
        [XmlElement(ElementName = "PaymentCard", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaymentCard PaymentCard { get; set; }
    }

    [XmlRoot(ElementName = "PaymentProcessingDetails", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaymentProcessingDetails
    {
        [XmlElement(ElementName = "Amount", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Amount Amount { get; set; }
        [XmlElement(ElementName = "Payer", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Payer Payer { get; set; }
        [XmlElement(ElementName = "PaymentMethod", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaymentMethod PaymentMethod { get; set; }
    }

    [XmlRoot(ElementName = "PaymentFunctions", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public class PaymentFunctions
    {
        [XmlElement(ElementName = "PaymentProcessingDetails", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaymentProcessingDetails PaymentProcessingDetails { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class Request
    {
        [XmlElement(ElementName = "BookingRef", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public BookingRef BookingRef { get; set; }
        [XmlElement(ElementName = "ChangeOrderChoice", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public ChangeOrderChoice ChangeOrderChoice { get; set; }
        [XmlElement(ElementName = "DataLists", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public DataLists DataLists { get; set; }
        [XmlElement(ElementName = "Order", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public Order Order { get; set; }
        [XmlElement(ElementName = "PaymentFunctions", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
        public PaymentFunctions PaymentFunctions { get; set; }
    }

    [XmlRoot(ElementName = "IATA_OrderChangeRQ", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    public class IATA_OrderChangeRQ
    {
        [XmlElement(ElementName = "DistributionChain", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public DistributionChain DistributionChain { get; set; }
        [XmlElement(ElementName = "Request", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
        public Request Request { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }
}