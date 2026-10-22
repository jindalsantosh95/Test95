using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class FlightDetailsResponse
    {

        public IEnumerable<FlightSegmentDetails> FlightSegmentDetails { get; set; }

        public ErrorResponse ErrorResponse { get; set; }


        public static explicit operator FlightDetailsResponse((OrderBookingResponse responseEmail, OrderBookingResponse responseFcNumber, IEnumerable<AirportInfo> airportDetails, IEnumerable<AirlineInfo> airlineDetails, string tierCode) input)
        {
            var responseEmail = input.responseEmail;
            var responseFcNumber = input.responseFcNumber;
            var airportDetails = input.airportDetails;
            var airlineDetails = input.airlineDetails;
            var tierCode = input.tierCode;

            List<FlightSegmentDetails> segmentDetailsList = new();

            ProcessFlightBookings(responseFcNumber?.FlightBookingsDetails, false);
            ProcessFlightBookings(responseEmail?.FlightBookingsDetails, !tierCode.Is(Application.VirginAccountTierCode));

            void ProcessFlightBookings(IEnumerable<FlightBookingDetails> flightBookings, bool showBanner)
            {
                if (flightBookings == null) return;

                foreach (var segment in flightBookings)
                {
                    var operatingAirline = airlineDetails?.FirstOrDefault(airline => airline.CarrierCode == segment.OperatingCarrier);
                    var originAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == segment.OriginAirportCode);
                    var destinationAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == segment.DestinationAirportCode);

                    segmentDetailsList.Add(new FlightSegmentDetails
                    {
                        BookingReference = segment.BookingReference,
                        OperatingAirlineName = operatingAirline?.CarrierName,
                        FlightNumber = $"{operatingAirline?.CarrierCode}{segment.OperatingFlightNumber.TrimStart('0')}",
                        OperatingAirlineCode = operatingAirline?.CarrierCode,
                        DateOfFlight = Common.Helper.DateTimeHelper.ParseVaaBookingDate(DateTime.Parse(segment.ScheduledDepartureTime).ToString(Application.ShortDateTimeFormat)).Value,
                        ScheduledDepartureTime = Common.Helper.DateTimeHelper.ParseVaaBookingTime(DateTime.Parse(segment.ScheduledDepartureTime).ToString(Application.TimeFormat)).Value,
                        NumberOfDaysToDeparture = (Convert.ToDateTime(segment.ScheduledDepartureTime).Subtract(DateTime.UtcNow.Date)).Days,
                        OriginAirportName = originAirport?.AirportName,
                        OriginAirportCode = originAirport?.AirportCode,
                        OriginAirportCountry = originAirport?.CountryName,
                        DestinationAirportName = destinationAirport?.AirportName,
                        DestinationAirportCode = destinationAirport?.AirportCode,
                        DestinationAirportCountry = destinationAirport?.CountryName,
                        ShowClaimsBanner = showBanner
                    });
                }
            }

            if (segmentDetailsList.Count == 0)
                return null;

            return new FlightDetailsResponse
            {
                FlightSegmentDetails = segmentDetailsList.GroupBy(s => new { s?.BookingReference, s?.FlightNumber, s?.DateOfFlight, s?.ScheduledDepartureTime }).Select(g => g.First()).OrderBy(s => s?.DateOfFlight).ThenBy(s => s?.ScheduledDepartureTime)
            };
        }
    }
}
