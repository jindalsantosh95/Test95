using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class TripDetails
    {
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public string? PnrLocator { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? FlyingClubNumber { get; set; }
        public string? BookingReference { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public static explicit operator TripDetails(Infrastructure.Models.TripMS.TripsDetailsRequest request)
        {
            if (request == null) throw null;

            return new()
            {
                GivenNames = request.GivenNames,
                Surname = request.Surname,
                PnrLocator = request.ConfirmationNum
            };
        }

        public static explicit operator TripDetails((FlightBookingDetail response, string fcNumber) source)
        {
            var (response, fcNumber) = source;
            if (response == null) return null;

            return new()
            {
                GivenNames = source.response.CustomerDetails?.FirstOrDefault()?.FirstName,
                Surname = source.response.CustomerDetails?.FirstOrDefault()?.LastName,
                PnrLocator = source.response.BookingReference,
                FlyingClubNumber = fcNumber
            };
        }

        public static explicit operator TripDetails(Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest request)
        {
            if (request == null) return null;

            return new()
            {
                Surname = request.Surname,
                BookingReference = request.BookingReference,
                DateOfBirth = request.DateOfBirth,
            };
        }
    }
}