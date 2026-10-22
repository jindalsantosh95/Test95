using System;
using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENENotification")]
    public class ENENotification
    {

        [XmlElement(ElementName = "reasonType")]
        public string ReasonType { get; set; }

        [XmlElement(ElementName = "clientApplicationCode")]
        public string ClientApplicationCode { get; set; }

        [XmlElement(ElementName = "recordLocator")]
        public string RecordLocator { get; set; }

        [XmlElement(ElementName = "bookingCreationGMTDate")]
        public DateTime BookingCreationGMTDate { get; set; }

        [XmlElement(ElementName = "flights")]
        public Flights Flights { get; set; }

        [XmlElement(ElementName = "fileAddress")]
        public string FileAddress { get; set; }

        [XmlElement(ElementName = "contacts")]
        public Contacts Contacts { get; set; }
    }

}