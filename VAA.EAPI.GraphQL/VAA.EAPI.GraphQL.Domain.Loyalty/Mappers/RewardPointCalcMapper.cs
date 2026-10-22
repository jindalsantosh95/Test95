using System;
using System.Globalization;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class RewardPointCalcMapper
    {
        public static PNRAndFareMutationResponse MapToPointCalcResponse(RewardBookingPointCalculationResponse pointResponse, Pnr pnr, string rloc)
        {
            var segments = pnr.SegmentDetails?.Where(seg => seg.DeparturePoint is not null && seg.ArrivalPoint is not null);

            return new PNRAndFareMutationResponse
            {
                PNR = rloc,
                TotalPoints = pointResponse?.TotalFarePoints,
                PaxFares = pointResponse.PaxFares.Any() ? pointResponse.PaxFares.Select(x => new PassengerFare
                {
                    CustomerId = x.CustomerId,
                    FirstName = x.FirstName,
                    FirstNameNumber = x.FirstNameNumber,
                    LastName = x.LastName,
                    LastNameNumber = x.LastNameNumber,
                    FarePoints = x.FarePoints,
                    Tickets = pnr.TicketInformation?.Where(t => t.FirstNameNumber == x.FirstNameNumber && t.LastNameNumber == x.LastNameNumber && t.TicketStatus != null)
                }) : pnr.Passengers?.Select(x => new PassengerFare
                {
                    CustomerId = x.CustomerId,
                    FirstName = x.FirstName,
                    FirstNameNumber = x.FirstNameNumber,
                    LastName = x.LastName,
                    LastNameNumber = x.LastNameNumber,
                    FarePoints = 0,
                    Tickets = pnr.TicketInformation?.Where(t => t.FirstNameNumber == x.FirstNameNumber && t.LastNameNumber == x.LastNameNumber && t.TicketStatus != null)
                }),
                NoOfSegments = segments?.Count() ?? 0,
                NoOfPassengers = pnr.Passengers?.Count() ?? 0,
                Segments = segments?.Select(segment => new SegmentDtl
                {
                    AirlineCode = segment.AirlineCode,
                    ArrivalAirport = segment.ArrivalPoint,
                    ClassOfService = segment.ClassOfService,
                    ClassOfService2 = segment.ClassOfService2,
                    DepartureDate = DateTime.ParseExact(segment.DepartureDate + segment.SegmentDepartureYear, Common.Constants.Application.ShortDateTimeFormat, CultureInfo.InvariantCulture).ToString(),
                    DepartureAirport = segment.DeparturePoint,
                    FlightNumber = segment.FlightNumber,
                    CurrentActionCode = segment.CurrentActionCode
                }),
            };
        }
    }
}
