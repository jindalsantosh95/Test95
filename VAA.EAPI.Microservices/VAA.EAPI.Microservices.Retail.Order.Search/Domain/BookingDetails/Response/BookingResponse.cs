using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response
{
    public class CustomerContact
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Countrycode { get; set; }
    }

    public class DtiSegmentInfo
    {
        public string BookNumber { get; set; }
        public string Carrier { get; set; }
        public string FareBasisFareClassCode { get; set; }
    }

    public class FarePrice
    {
        public List<FarePriceDetail> FarePriceDetails { get; set; }
        public List<DtiSegmentInfo> DtiSegmentInfo { get; set; }
    }

    public class FarePriceDetail
    {
        public string BaseCurrency { get; set; }
        public string TotalFareCurrency { get; set; }
    }

    public class FrequentFlyer
    {
        public string AccountNumber { get; set; }
        public string AirlineCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OsiNumber { get; set; }
    }

    public class OriginalPos
    {
        public string AgencyId { get; set; }
        public string AgentId { get; set; }
    }

    public class BookingResponse
    {
        public List<VaaCustomerDetail> VaaCustomerDetails { get; set; }
        public List<VaaBookingDetail> VaaBookingDetails { get; set; }
    }

    public class TotalFeesAndTaxes
    {
        public string BaseCurrency { get; set; }
    }

    public class VaaBookingCreationDateTime
    {
        public string LocalDateTime { get; set; }
        public string UtcDateTime { get; set; }
    }

    public class VaaBookingDetail
    {
        public string OrderId { get; set; }
        public string IsOldBooking { get; set; }
        public FarePrice FarePrice { get; set; }
        public TotalFeesAndTaxes TotalFeesAndTaxes { get; set; }
        public VaaBookingCreationDateTime VaaBookingCreationDateTime { get; set; }
        public string Rloc { get; set; }
        public string RlocCreateDate { get; set; }
        public OriginalPos OriginalPos { get; set; }
        public List<VaaBookingSegmentDetail> VaaBookingSegmentDetails { get; set; }
    }

    public class VaaBookingSegmentDetail
    {
        public string ArrivalPoint { get; set; }
        public string DeparturePoint { get; set; }
        public string MarketingCarrier { get; set; }
        public string OperatingCarrier { get; set; }
        public string ClassOfService { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string FlightNumber { get; set; }
    }

    public class VaaCustomerDetail
    {
        public string VaaScvId { get; set; }
        public string ContactTypeText { get; set; }
        public string Title { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public string BookingType { get; set; }
        public FrequentFlyer FrequentFlyer { get; set; }
        public List<CustomerContact> CustomerContacts { get; set; }
    }
}
