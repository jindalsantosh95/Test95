using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using static VAA.EAPI.GraphQL.Domain.Account.Models.BookingDetailsResponse;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class FlightDetailsResponseMapper
    {
        public static async Task<FlightDetailsResponse> MapFlightDetailsResponseMapper(ILogger<FlightQueryProvider> logger, string fcNumber, BookingResponse response, IRefDataService<AirportInfo> airportRefData, IRefDataService<AirlineInfo> airlineRefData)
        {

            if (response.VaaBookingDetails.Any())
            {
                var airportDetails = await airportRefData.Query();
                var airlineDetails = await airlineRefData.Query();
                IEnumerable<FlightSegmentDetails> segmentDetailsList = null;

                var booking = response.VaaBookingDetails.SelectMany(x => x.VaaBookingSegmentDetails, (rloc, segmentdetail) => new { rloc = rloc.Rloc, segmentdetail })
               .Where(s => Common.Helper.DateTimeHelper.ParseVaaBookingDate(s.segmentdetail.DepartureDate).Value.ToDateTimeUnspecified().Date >= DateTime.UtcNow.Date);

                segmentDetailsList = booking.Select(vaaSegmentDetails =>
                {
                    var operatingAirline = airlineDetails?.FirstOrDefault(airline => airline.CarrierCode == vaaSegmentDetails.segmentdetail.OperatingCarrier);
                    var originAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == vaaSegmentDetails.segmentdetail.DeparturePoint);
                    var destinationAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == vaaSegmentDetails.segmentdetail.ArrivalPoint);

                    return new FlightSegmentDetails
                    {
                        BookingReference = vaaSegmentDetails.rloc,
                        OperatingAirlineName = operatingAirline?.CarrierName,
                        FlightNumber = $"{operatingAirline.CarrierCode}{vaaSegmentDetails.segmentdetail.FlightNumber.TrimStart('0')}",
                        OperatingAirlineCode = operatingAirline.CarrierCode,
                        DateOfFlight = Common.Helper.DateTimeHelper.ParseVaaBookingDate(vaaSegmentDetails.segmentdetail.DepartureDate).Value,
                        ScheduledDepartureTime = Common.Helper.DateTimeHelper.ParseVaaBookingTime(vaaSegmentDetails.segmentdetail.DepartureTime).Value,
                        NumberOfDaysToDeparture = (Convert.ToDateTime(vaaSegmentDetails.segmentdetail.DepartureDate).Subtract(DateTime.UtcNow.Date)).Days,
                        OriginAirportName = originAirport?.AirportName,
                        OriginAirportCode = originAirport?.AirportCode,
                        OriginAirportCountry = originAirport?.CountryName,
                        DestinationAirportName = destinationAirport?.AirportName,
                        DestinationAirportCode = destinationAirport?.AirportCode,
                        DestinationAirportCountry = destinationAirport?.CountryName,
                    };
                });

                FlightDetailsResponse flightDetailsResponse = new FlightDetailsResponse()
                {
                    FlightSegmentDetails = segmentDetailsList
                    .Where(x => !string.IsNullOrEmpty(x.BookingReference) && !string.IsNullOrEmpty(x.OperatingAirlineName)
                    && !string.IsNullOrEmpty(x.FlightNumber) && !string.IsNullOrEmpty(x.OperatingAirlineCode)
                    && !string.IsNullOrEmpty(x.OriginAirportCode) && !string.IsNullOrEmpty(x.OriginAirportName)
                    && !string.IsNullOrEmpty(x.OriginAirportCountry) && !string.IsNullOrEmpty(x.DestinationAirportCountry)
                    && !string.IsNullOrEmpty(x.DestinationAirportCode) && !string.IsNullOrEmpty(x.DestinationAirportName))
                    .OrderBy(seg => seg.DateOfFlight)
                };
                if (flightDetailsResponse.FlightSegmentDetails.Count() != segmentDetailsList.Count())
                {
                    logger.LogInformation($"For FcNumber: {fcNumber}: {segmentDetailsList.Count() - flightDetailsResponse.FlightSegmentDetails.Count()} data are suppressed");

                }
                return flightDetailsResponse;
            }
            return null;
        }
    }
}
