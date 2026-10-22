using VAA.EAPI.Microservices.Retail.Holidays.Domain;

namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{

    public class HolidayTripListResponseItem
    {
        public RetrieveBookingRequest HolidayRequest { get; set; }
        public HolidayTripResponse HolidayResponse { get; set; }
    }
}
