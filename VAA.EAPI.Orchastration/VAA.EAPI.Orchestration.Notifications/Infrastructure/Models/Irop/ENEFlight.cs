using System;
using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENEFlight")]
    public class ENEFlight
    {

        [XmlElement(ElementName = "marketedCarrierCode")]
        public string MarketedCarrierCode { get; set; }

        [XmlElement(ElementName = "marketedFlightNumber")]
        public int MarketedFlightNumber { get; set; }

        [XmlElement(ElementName = "scheduledSegmentDepartureLocalTimestamp")]
        public string ScheduledSegmentDepartureLocalTimestamp { get; set; }

        [XmlElement(ElementName = "originLocalDate")]
        public DateTime OriginLocalDate { get; set; }

        [XmlElement(ElementName = "scheduledSegmentArrivalLocalTimestamp")]
        public string ScheduledSegmentArrivalLocalTimestamp { get; set; }

        [XmlElement(ElementName = "origin")]
        public string Origin { get; set; }

        [XmlElement(ElementName = "destination")]
        public string Destination { get; set; }

        [XmlElement(ElementName = "detailReasonTypeCode")]
        public DetailReasonTypeCode DetailReasonTypeCode { get; set; }

        [XmlElement(ElementName = "seats")]
        public Seats Seats { get; set; }

        [XmlElement(ElementName = "bookingClass")]
        public string BookingClass { get; set; }

        [XmlElement(ElementName = "bookingSegmentNumber")]
        public int BookingSegmentNumber { get; set; }

        [XmlElement(ElementName = "actualDepartureDifferenceMinuteCount")]
        public int ActualDepartureDifferenceMinuteCount { get; set; }

        [XmlElement(ElementName = "actualArrivalDifferenceMinuteCount")]
        public int ActualArrivalDifferenceMinuteCount { get; set; }

        [XmlElement(ElementName = "compartmentCode")]
        public int CompartmentCode { get; set; }

        [XmlElement(ElementName = "refunds")]
        public object Refunds { get; set; }
    }

}