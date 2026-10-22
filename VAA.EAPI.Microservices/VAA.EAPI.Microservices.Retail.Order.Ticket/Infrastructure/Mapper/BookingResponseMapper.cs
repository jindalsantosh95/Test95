using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Mapper
{
    public class BookingResponseMapper
    {

        public static IssueTicketResponse ResponseMapper(BookingDetails bookingDetails)
        {
            return new IssueTicketResponse()
            {
                SegmentDetails = bookingDetails.SegmentDetails,
                AccountableDocumentRef = bookingDetails.AccountableDocumentRef,
                CustomerDetails = PassengerInformation(bookingDetails),
                TicketSegmentDetails = TicketSegmentInfo(bookingDetails),
            };
        }


        private static List<Customer> PassengerInformation(BookingDetails bookingDetails)
        {
            List<Customer> PassengerIfo = new();
            foreach (var psg in bookingDetails.Passengers)
            {
                PassengerIfo.Add(new Customer
                {

                    CustId = psg.CustomerId,
                    FirstName = psg.FirstName,
                    FirstNameNumber = psg.FirstNameNumber,
                    LastNameNumber = psg.LastNameNumber,
                    LastName = psg.LastName,
                    Title = psg.Title,
                });
            }
            return PassengerIfo;
        }
        private static List<TicketSegment> TicketSegmentInfo(BookingDetails bookingDetails)
        {
            List<TicketSegment> ticketSegmentInfo = new();
            foreach (var ticketSegment in bookingDetails.TicketSegments)
            {
                ticketSegmentInfo.Add(new TicketSegment
                {
                    ClassOfService = ticketSegment.ClassOfService,
                    MarketingCarrier = ticketSegment.MarketingCarrier,
                    MarketingFlightNumber = ticketSegment.MarketingFlightNumber,
                    OriginAirportCode = ticketSegment.OriginAirportCode,
                    SegmentNumber = ticketSegment.SegmentNumber,
                    ScheduledDepartureTime = new()
                    {
                        LocalDateTime = ticketSegment.ScheduledDepartureTime.LocalDateTime,
                        UtcDateTime = string.Empty
                    },
                    Legs = null,
                    ExpandedPassengerTypeCode = ticketSegment.ExpandedPassengerTypeCode,
                    PassengerTypeCode = ticketSegment.PassengerTypeCode,
                    FareBasis = ticketSegment.FareBasis
                });
            }
            return ticketSegmentInfo;
        }

    }
}
