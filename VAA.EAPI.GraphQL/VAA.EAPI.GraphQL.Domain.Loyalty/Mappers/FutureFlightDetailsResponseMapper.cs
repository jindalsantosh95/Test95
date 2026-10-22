using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class FutureFlightDetailsResponseMapper
    {
        public static FlightResponse MapFlightDetailsResponseMapper(ILogger<FutureFlightsQueryProvider> logger, string fcNumber, BookingDetailsResponse response, IEnumerable<AirportInfo> airportDetails, IEnumerable<AirlineInfo> airlineDetails)
        {
            if (!string.IsNullOrEmpty(response.Message)) return new FlightResponse { ErrorResponses = new ErrorResponses { Message = response.Message } };

            if (response.FlightBookingsDetails != null && response.FlightBookingsDetails.Any())
            {
                IEnumerable<FlightSegmentDetail> segmentDetailsList = null;

                segmentDetailsList = response.FlightBookingsDetails.Select(vaaSegmentDetails =>
                {
                    var operatingAirline = airlineDetails?.FirstOrDefault(airline => airline.CarrierCode == vaaSegmentDetails.OperatingCarrier);
                    var marketingAirline = airlineDetails?.FirstOrDefault(airline => airline.CarrierCode == vaaSegmentDetails.MarketingCarrier);
                    var originAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == vaaSegmentDetails.OriginAirportCode);
                    var destinationAirport = airportDetails?.FirstOrDefault(airport => airport.AirportCode == vaaSegmentDetails.DestinationAirportCode);

                    return new FlightSegmentDetail
                    {
                        BookingReference = vaaSegmentDetails.BookingReference,
                        OperatingAirlineName = operatingAirline?.CarrierName,
                        FlightNumber = $"{operatingAirline?.CarrierCode}{vaaSegmentDetails.OperatingFlightNumber.TrimStart('0')}",
                        OperatingAirlineCode = operatingAirline?.CarrierCode,
                        DateOfFlight = Common.Helper.DateTimeHelper.ParseVaaBookingDate(DateTime.Parse(vaaSegmentDetails.ScheduledDepartureTime).ToString(Application.ShortDateTimeFormat)).Value,
                        ScheduledDepartureTime = Common.Helper.DateTimeHelper.ParseVaaBookingTime(DateTime.Parse(vaaSegmentDetails.ScheduledDepartureTime).ToString(Application.TimeFormat)).Value,
                        NumberOfDaysToDeparture = (Convert.ToDateTime(vaaSegmentDetails.ScheduledDepartureTime).Subtract(DateTime.UtcNow.Date)).Days,
                        OriginAirportName = originAirport?.AirportName,
                        OriginAirportCode = originAirport?.AirportCode,
                        OriginAirportCountry = originAirport?.CountryName,
                        DestinationAirportName = destinationAirport?.AirportName,
                        DestinationAirportCode = destinationAirport?.AirportCode,
                        DestinationAirportCountry = destinationAirport?.CountryName,
                        MarketingAirlineName = marketingAirline?.CarrierName,
                        MarketingAirlineCode = marketingAirline?.CarrierCode,
                        MarketingFlightNumber = $"{marketingAirline?.CarrierCode}{vaaSegmentDetails.MarketingFlightNumber.TrimStart('0')}",
                        BookingClass = vaaSegmentDetails.BookingClass,
                    };
                });

                FlightResponse flightDetailsResponse = new FlightResponse()
                {
                    FlightSegmentDetails = segmentDetailsList
                    .Where(x => !string.IsNullOrEmpty(x.BookingReference) && !string.IsNullOrEmpty(x.OperatingAirlineName)
                    && !string.IsNullOrEmpty(x.FlightNumber) && !string.IsNullOrEmpty(x.OperatingAirlineCode)
                    && !string.IsNullOrEmpty(x.OriginAirportCode) && !string.IsNullOrEmpty(x.OriginAirportName)
                    && !string.IsNullOrEmpty(x.OriginAirportCountry) && !string.IsNullOrEmpty(x.DestinationAirportCountry)
                    && !string.IsNullOrEmpty(x.DestinationAirportCode) && !string.IsNullOrEmpty(x.DestinationAirportName))
                    .OrderBy(seg => seg.DateOfFlight)
                };

                return flightDetailsResponse;
            }

            return null;
        }
    }
}
