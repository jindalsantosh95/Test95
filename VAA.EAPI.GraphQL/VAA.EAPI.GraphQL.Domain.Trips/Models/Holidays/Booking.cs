using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayBooking")]
    public class Booking
    {
        public BookingPayment? Payment { get; set; }
        public Location Destination { get; set; }
        public TravelDatesDetails TravelDates { get; set; }
        public HolidayBookingSummary? HolidayBookingSummary { get; set; }

    }
}
