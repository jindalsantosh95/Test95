using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderChangeResponse
    {
        public string Rloc { get; set; }
        public OrderDetails OrderDetails { get; set; }

        public static explicit operator OrderChangeResponse(SeatDeletionResponse seatDeletionResponse)
        {
            return new OrderChangeResponse()
            {
                OrderDetails = new OrderDetails()
                {
                    AccountableDocumentRefs = seatDeletionResponse.OrderItems.Select(x => new AccountableDocumentReference()
                    {
                        CurrentSeatAssignment = x.SeatNumber,
                        SegmentRefId = x.FlightIdRef
                    })
                }
            };
        }

    }
}
