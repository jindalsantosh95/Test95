using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PassengerRetrieval;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper.PassengerRetrieval
{
    public static class PassengerFlightResponseMapper
    {
        public static IEnumerable<PassengerRetrievalResponse> CreateResponses(OrderDoc orderDoc, PassengerRetrievalQuery request)
        {
            var segment = orderDoc.SegmentDetails?.FirstOrDefault(segment => SegmentMatchesRequest(segment, request));
            var eventFlight = CreateFlightDetails(request, segment);
            var passengers = orderDoc.CustomerDetails ?? Enumerable.Empty<CustomerDetail>();

            return passengers.Select(passenger =>
            {
                var ticket = GetTicket(orderDoc, passenger);
                var response = (PassengerRetrievalResponse)orderDoc;

                response.EventFlight = eventFlight;
                response.Passengers = new[]
                {
                    new Passenger
                    {
                        FirstName = passenger.FirstName,
                        LastName = passenger.LastName,
                        Email = passenger.CustomerContacts?.Contacts?.FirstOrDefault(contact => !string.IsNullOrWhiteSpace(contact.Email))?.Email,
                        PassengerType = passenger.PTC
                    }
                };
                response.Seats = orderDoc.Fulfillment?.Seat?
                    .Where(seat => SeatMatchesPassengerAndRequest(seat, passenger, request))
                    .ToArray()
                    ?? Array.Empty<Seat>();
                response.TicketType = ticket?.TicketType;
                response.TicketDesignator = ticket?.TicketingTypeCode;
                response.TicketNumber = ticket?.TicketNumber;
                response.PnrName = ticket?.PassengerName;
                response.TicketIssueDate = ticket?.IssueDate;
                response.TicketOriginalCity = ticket?.OriginalTicketCity;
                response.OperatingAirAuthNumber = ticket?.ApprovalCode;

                var frequentFlyerDetails = FrequentFlyerMapper.Resolve(orderDoc.SpecialServiceRequest, passenger, ticket);
                response.LoyaltyAccountNumber = frequentFlyerDetails.MemberNumber;
                response.LoyaltyAirlineCode = frequentFlyerDetails.AirlineCode;
                response.ProcessingStatus = ticket?.TicketStatus;
                response.CouponNumber = GetCouponNumber(ticket, eventFlight);
                response.FareBasisCode = GetFareBasisCode(orderDoc, passenger, eventFlight);

                return response;
            });
        }

        public static string CreatePassengerFlightKey(PassengerRetrievalResponse response)
        {
            var passenger = response.Passengers?.FirstOrDefault();
            var flight = response.EventFlight;

            return string.Join("|",
                response.OrderId,
                passenger?.FirstName,
                passenger?.LastName,
                passenger?.PassengerType,
                response.TicketNumber,
                flight?.FlightNumber,
                flight?.DepartureAirport,
                flight?.ArrivalAirport,
                flight?.DepartureDate.ToString("O"));
        }

        private static Ticket? GetTicket(OrderDoc orderDoc, CustomerDetail passenger) =>
            orderDoc.Fulfillment?.Tickets?.FirstOrDefault(ticket =>
                ticket.FirstNameNumber == passenger.FirstNameNumber &&
                ticket.LastNameNumber == passenger.LastNameNumber)
            ?? orderDoc.Fulfillment?.Tickets?.FirstOrDefault();

        private static bool SeatMatchesRequest(Seat seat, PassengerRetrievalQuery request)
        {
            var requestFlightNumber = request.FlightNumber?.TrimStart('0');
            var seatFlightNumber = seat.FlightNumber?.TrimStart('0');

            if (!requestFlightNumber.Is(seatFlightNumber)) return false;

            var departureMatches = seat.DeparturePoint.Is(request.DepartureAirport) || seat.SeatDeparturePoint.Is(request.DepartureAirport);
            var arrivalMatches = seat.ArrivalPoint.Is(request.ArrivalAirport);

            if (!departureMatches || !arrivalMatches) return false;

            if (!seat.DepartureDate.IsNullOrWhiteSpace() && !seat.DepartureYear.IsNullOrWhiteSpace())
            {
                var expectedDepartureDate = request.DepartureDate.ToString("ddMMM", CultureInfo.InvariantCulture).ToUpperInvariant();
                var expectedDepartureYear = seat.DepartureYear.Length == 4
                    ? request.DepartureDate.ToString("yyyy", CultureInfo.InvariantCulture)
                    : request.DepartureDate.ToString("yy", CultureInfo.InvariantCulture);

                if (!expectedDepartureDate.Is(seat.DepartureDate) || !expectedDepartureYear.Is(seat.DepartureYear)) return false;
            }

            return true;
        }

        private static bool SeatMatchesPassengerAndRequest(Seat seat, CustomerDetail passenger, PassengerRetrievalQuery request) =>
            SeatMatchesRequest(seat, request) &&
            (string.IsNullOrWhiteSpace(seat.FirstNameNumber) || string.Equals(seat.FirstNameNumber, passenger.FirstNameNumber, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(seat.LastNameNumber) || string.Equals(seat.LastNameNumber, passenger.LastNameNumber, StringComparison.OrdinalIgnoreCase));

        private static PassengerRetrievalQuery CreateFlightDetails(PassengerRetrievalQuery request, SegmentDetail? segment) =>
            new()
            {
                FlightCarrier = request.FlightCarrier,
                FlightNumber = request.FlightNumber,
                DepartureAirport = request.DepartureAirport,
                ArrivalAirport = request.ArrivalAirport,
                DepartureDate = request.DepartureDate,
                OperatingCarrier = segment?.OperatingCarrier,
                OperatingFlightNumber = segment?.OperatingFlightNumber,
                MarketingCarrier = segment?.MarketingCarrier,
                MarketingFlightNumber = segment?.MarketingFlightNumber,
                BookingClass = segment?.ClassOfService,
                CabinClass = segment?.CompartmentNumber
            };

        private static string? GetCouponNumber(Ticket? ticket, PassengerRetrievalQuery eventFlight) =>
            ticket?.Coupon?.FirstOrDefault(coupon =>
                coupon.DeparturePoint.Is(eventFlight.DepartureAirport) &&
                coupon.ArrivalPoint.Is(eventFlight.ArrivalAirport))?.CouponNumber;

        private static string GetFareBasisCode(OrderDoc orderDoc, CustomerDetail passenger, PassengerRetrievalQuery eventFlight)
        {
            var flightNumber = eventFlight.MarketingFlightNumber ?? eventFlight.FlightNumber;
            var fareSegments = orderDoc.OrderDetails?.FarePrice?.DtiSegmentInfo;
            var fareSegment = fareSegments?.FirstOrDefault(segment =>
                string.Equals(segment.FlightNumber?.TrimStart('0'), flightNumber?.TrimStart('0'), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(segment.DeparturePoint, eventFlight.DepartureAirport, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(segment.PassengerTypeCode, passenger.PTC, StringComparison.OrdinalIgnoreCase));

            return fareSegment?.FarebasisFareclassCode ?? string.Empty;
        }

        private static bool SegmentMatchesRequest(SegmentDetail segment, PassengerRetrievalQuery request)
        {
            var requestFlightNumber = request.FlightNumber?.TrimStart('0');
            var operatingFlightNumber = segment.OperatingFlightNumber?.TrimStart('0');
            var marketingFlightNumber = segment.MarketingFlightNumber?.TrimStart('0');

            if (!requestFlightNumber.Is(operatingFlightNumber) && !requestFlightNumber.Is(marketingFlightNumber)) return false;

            var departureMatches = segment.OriginAirportCode.Is(request.DepartureAirport) || segment.DeparturePoint.Is(request.DepartureAirport);
            var arrivalMatches = segment.DestinationAirportCode.Is(request.ArrivalAirport) || segment.ArrivalPoint.Is(request.ArrivalAirport);

            return departureMatches && arrivalMatches;
        }
    }
}
