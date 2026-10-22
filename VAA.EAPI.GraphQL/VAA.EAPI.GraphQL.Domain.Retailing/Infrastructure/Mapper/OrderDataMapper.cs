using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Mapper
{
    public static class OrderDataMapper
    {
        public static FlyingClubBookings PnrMapper(string fcNumber, FlightBookingsResponse response)
        {
            var custInfo = response.FlightBookingsDetails.SelectMany(x => x.CustomerDetails ?? Enumerable.Empty<CustomerDetail>()).FirstOrDefault();
            return new FlyingClubBookings
            {
                FCNumber = fcNumber,
                FirstName = custInfo is null ? string.Empty : custInfo.FirstName,
                LastName = custInfo is null ? string.Empty : custInfo.LastName,
                Bookings = response.FlightBookingsDetails.Select(pnr =>
                new PNR
                {
                    BookingDate = Convert.ToDateTime(pnr.BookingDate),
                    RecordLocator = pnr.BookingReference,
                    Segments = [
                        new() {
                        SegmentID = pnr.SegmentNumber,
                        MarketingFlight = new FlightNumber
                        {
                            AirlineCode = pnr.MarketingCarrier,
                            Number = Convert.ToInt32(pnr.MarketingFlightNumber)
                        },
                        OperatingFlight = new FlightNumber
                        {
                            AirlineCode = pnr.OperatingCarrier,
                            Number = Convert.ToInt32(pnr.OperatingFlightNumber)
                        },
                        Origin = pnr.OriginAirportCode,
                        Destination = pnr.DestinationAirportCode,
                        Departure = Convert.ToDateTime(pnr.ScheduledDepartureTime),
                        Cabin = pnr.BookingClass,
                    } ],
                    Passengers = pnr.CustomerDetails?.Select(pax =>
                         new Passenger
                         {
                             FirstName = pax.FirstName,
                             LastName = pax.LastName,
                             PassengerId = string.Concat(pax.FirstNameNumber, ".", pax.LastNameNumber),
                             FrequentFlyerNumber = pax.LoyaltyMemberDetails?.MemberNumber,
                         }).ToList()
                })
            };
        }

    }
}
