using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PassengerRetrieval
{
    public class PassengerRetrievalResponse
    {
        public PassengerRetrievalQuery EventFlight { get; set; }
        public FlightOrderNotification.BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
        public string OrderId { get; set; }
        public bool Irop { get; set; }
        public string RecordType { get; set; }
        public string PointOfSale { get; set; }
        public string AgentIataNumber { get; set; }
        public string TicketType { get; set; }
        public string TicketDesignator { get; set; }
        public string TicketNumber { get; set; }
        public string PnrName { get; set; }
        public string TicketIssueDate { get; set; }
        public string TicketOriginalCity { get; set; }
        public string OperatingAirAuthNumber { get; set; }
        public string LoyaltyAccountNumber { get; set; }
        public string LoyaltyAirlineCode { get; set; }
        public string CouponNumber { get; set; }
        public string FareBasisCode { get; set; }
        public string ProcessingStatus { get; set; }

        public static explicit operator PassengerRetrievalResponse(OrderDoc orderDoc)
        {
            var passenger = orderDoc?.CustomerDetails?.FirstOrDefault(cd => cd.CustomerContacts?.Contacts?
                            .Any(c => !string.IsNullOrWhiteSpace(c.Email)) == true) ?? orderDoc?.CustomerDetails?.FirstOrDefault();
            var ticket = orderDoc?.Fulfillment?.Tickets?.FirstOrDefault(t =>
                             t.FirstNameNumber == passenger?.FirstNameNumber &&
                             t.LastNameNumber == passenger?.LastNameNumber)
                         ?? orderDoc?.Fulfillment?.Tickets?.FirstOrDefault();

            return new PassengerRetrievalResponse
            {
                BookingReference = (FlightOrderNotification.BookingReference)orderDoc,
                Passengers = passenger == null ? null : new List<Passenger>
                                {
                                new()
                                {
                                    FirstName = passenger?.FirstName,
                                    LastName = passenger?.LastName,
                                    Email = passenger?.CustomerContacts?.Contacts?.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Email))?.Email,
                                    PassengerType = passenger?.PTC,
                                }
                            },
                OrderId = orderDoc?.OrderID,
                Irop = orderDoc?.OrderHistorydetails?.Remarks?.Any(r => r.FreeformText?.Contains("IROP") ?? false) ?? false,
                RecordType = orderDoc?.DocType,
                PointOfSale = ticket?.OriginalSalesLocation,
                AgentIataNumber = orderDoc?.OriginalPOS?.AgencyId,
                TicketType = ticket?.TicketType,
                TicketDesignator = ticket?.TicketingTypeCode,
                TicketNumber = ticket?.TicketNumber,
                PnrName = ticket?.PassengerName,
                TicketIssueDate = ticket?.IssueDate,
                TicketOriginalCity = ticket?.OriginalTicketCity,
                OperatingAirAuthNumber = ticket?.ApprovalCode,
                LoyaltyAccountNumber = passenger?.FrequentFlyer?.AccountNumber,
                LoyaltyAirlineCode = passenger?.FrequentFlyer?.AirlineCode,
                ProcessingStatus = ticket?.TicketStatus

            };
        }
    }
}
