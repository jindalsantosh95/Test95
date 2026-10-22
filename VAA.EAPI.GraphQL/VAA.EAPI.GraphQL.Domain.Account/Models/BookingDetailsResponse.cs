namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class BookingDetailsResponse
    {
        public class BookingResponse
        {
            public IEnumerable<VaaBookingDetail> VaaBookingDetails { get; set; }
        }

        public class VaaBookingDetail
        {
            public string Rloc { get; set; }

            public IEnumerable<VaaBookingSegmentDetail> VaaBookingSegmentDetails { get; set; }
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


    }
}
