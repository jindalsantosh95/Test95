using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsCreateBasketRequest")]
    public class CreateBasketRequest
    {
        public Guid BasketId { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public Booking? BookingDetails { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }

        public static explicit operator CreateBasketRequest((TripMS.Booking booking, Guid basketId, IEnumerable<AirportRefData> airports, string email) source)
        {
            var (booking, basketId, airports, email) = source;

            var segments = booking.Product?.Where(p => p.BookingType == "AIRLINE" && p.Slices != null).SelectMany(p => p.Slices).SelectMany(s => s.Segments);

            var seats = booking.Passengers
                .SelectMany(passenger => (passenger.Slices ?? Enumerable.Empty<Slice>())
                .SelectMany(slice => (slice.Segments ?? Enumerable.Empty<Segment>())
                .SelectMany(segment => (segment.Legs ?? Enumerable.Empty<Leg>())
                .SelectMany(leg => (leg.SeatAssignments ?? Enumerable.Empty<TripMS.SeatAssignment>())
                .Where(seat => seat.Seat != null).Select(ns => (SeatAssignment)(ns, segment.Id, passenger.PassengerId))))));

            return new()
            {
                BasketId = basketId,
                BookingDetails = new()
                {
                    PnrLocator = booking.Product?.FirstOrDefault(p => p.BookingType == "AIRLINE")?.Reference?.PnrLocator,
                    Pos = new() { CountryCode = booking.Pos.CountryCode, CurrencyCode = booking.Pos.CurrencyCode ?? Constants.Application.DefaultCurrencyCode, CityCode = airports.FirstOrDefault(city => city.AirportCode == booking.Product?.FirstOrDefault(p => p.BookingType == "AIRLINE")?.Slices?.FirstOrDefault()?.Segments?.FirstOrDefault()?.Origin.Code)?.CityCode },
                    Passenger = booking.Passengers.Select(p => (Person)p),
                    Slices = booking.Product?.Where(p => p.BookingType == "AIRLINE" && p.Slices != null).SelectMany(p => p.Slices).Select(p => new Slice()
                    {
                        Id = p.Id,
                        ScheduledDepartureLocalDateTime = p.ScheduledDepartureLocalDateTime,
                        ScheduledArrivalLocalDateTime = p.ScheduledArrivalLocalDateTime,
                        Origin = p.Origin,
                        Destination = p.Destination,
                        Segments = p.Segments.Select(seg => new Segment()
                        {
                            Id = seg.Id,
                            OperatingCarrier = seg.OperatingCarrier,
                            MarketingCarrier = seg.MarketingCarrier,
                            ScheduledDepartureLocalDateTime = seg.ScheduledDepartureLocalDateTime,
                            ScheduledArrivalLocalDateTime = seg.ScheduledArrivalLocalDateTime,
                            Origin = seg.Origin,
                            Destination = seg.Destination,
                            FareBasisCode = seg.FareBasisCode,
                            Legs = seg.Legs.Select(l => new Leg()
                            {
                                Id = l.Id,
                                Operating = l.Operating,
                                Marketing = l.Marketing,
                                ScheduledDepartureLocalDateTime = l.ScheduledDepartureLocalDateTime,
                                ScheduledArrivalLocalDateTime = l.ScheduledArrivalLocalDateTime,
                                Origin = l.Origin,
                                Destination = l.Destination,
                                CabinClass = l.CabinClass,
                                Aircraft = l.Aircraft
                            })
                        })
                    }),
                    SeatDetails = seats
                },
                ContactDetails = new() { EmailAddress = email },
                FlightPassengerSeatMap = booking.Passengers.Where(passenger => passenger.PassengerType?.Code != "INF").SelectMany(passenger => segments.Select(segment => (FlightPassengerSeatMap)(segment, passenger)))
            };
        }
    }
}