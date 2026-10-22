using HotChocolate;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Constants;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatMSDeletionRequest")]
    public class SeatDeletionRequest
    {
        public Pos Pos { get; set; }       
        public string Rloc { get; set; }      
        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
        public IEnumerable<OrderAmendment> OrderAmendments { get; set; }
        public IEnumerable<SeatTransactionId> SeatTransactionId { get; set; }
        public string ChannelId { get; set; }
        public string ExternalRloc { get; set; }

        public static explicit operator SeatDeletionRequest((Basket basket, IEnumerable<BasketMS.SeatAssignment> seatDeletionRequest) source)
        {
            var (basket, seatDeletionRequest) = source;
            var distinctPassengerIds = seatDeletionRequest.Select(s => s.PassengerId).Distinct();
            return new()
            {
                Pos = basket.BookingDetails.Pos,
                Rloc = basket.BookingDetails.PnrLocator,
                CustomerDetails = distinctPassengerIds.Select(c =>
                {
                    var passenger = basket.BookingDetails.Passenger.FirstOrDefault(p => p.Id == c);
                    var customer = passenger?.CustomerId.Split('-');
                    return new CustomerDetail
                    {
                        CustId = passenger.CustomerId,
                        FirstNameNumber = customer.Last(),
                        LastNameNumber = customer.First(),
                    };
                }),
                OrderAmendments = distinctPassengerIds.Select(c =>
                {
                    var passenger = basket.BookingDetails.Passenger.Where(p => p.Id == c);
                    return new OrderAmendment
                    {
                        ProductType = ProductTypes.Seat.ToString(),
                        FlightItems = seatDeletionRequest.Where(fl => fl.PassengerId == c).Select(a => (FlightItems)(basket, a)),
                        CustomerReference = passenger.Select(p => p.CustomerId)
                    };
                }),
                SeatTransactionId = seatDeletionRequest.Select(s => new SeatTransactionId
                {
                    SegmentId = s.SegmentId,
                    TransactionId = AsyncContext.Current.CorrelationId.ToString(),
                }),
                ChannelId = Constants.Application.SeatMSDLChannelId,
                ExternalRloc = basket.BookingDetails.ExternalRloc ?? basket.BookingDetails.PnrLocator
            };
        }
    }
}