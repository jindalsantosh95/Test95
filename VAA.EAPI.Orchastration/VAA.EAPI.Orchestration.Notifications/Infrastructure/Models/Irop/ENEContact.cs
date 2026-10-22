using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENEContact")]
    public class ENEContact
    {

        [XmlElement(ElementName = "recipientFirstName")]
        public string RecipientFirstName { get; set; }

        [XmlElement(ElementName = "recipientLastName")]
        public string RecipientLastName { get; set; }

        [XmlElement(ElementName = "contactMethodCode")]
        public string ContactMethodCode { get; set; }

        [XmlElement(ElementName = "addressId")]
        public string AddressId { get; set; }

        [XmlElement(ElementName = "vendorCode")]
        public string VendorCode { get; set; }

        [XmlElement(ElementName = "statusCodes")]
        public StatusCodes StatusCodes { get; set; }

        [XmlElement(ElementName = "customerSourceSystemId")]
        public string CustomerSourceSystemId { get; set; }

        [XmlElement(ElementName = "clientApplicationTrackingID")]
        public double ClientApplicationTrackingID { get; set; }

        [XmlElement(ElementName = "qaUseOverrideIndicator")]
        public string QaUseOverrideIndicator { get; set; }

        [XmlElement(ElementName = "addressFormatTypeCode")]
        public string AddressFormatTypeCode { get; set; }

        [XmlElement(ElementName = "ENENotify")]
        public string ENENotify { get; set; }

        [XmlElement(ElementName = "affectedPassenger")]
        public AffectedPassenger AffectedPassenger { get; set; }
    }

}