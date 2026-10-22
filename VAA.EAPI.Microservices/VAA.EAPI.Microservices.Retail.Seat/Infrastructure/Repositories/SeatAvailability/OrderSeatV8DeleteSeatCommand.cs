using MediatR;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class OrderSeatV8DeleteSeatCommand : OrderChangeRequestData, IRequest<SeatDeletionResponse>
    {
        public string SessionId { get; set; }


        public static explicit operator OrderSeatV8DeleteSeatCommand((OrderSeatV8DeleteSeatCommand request, Segment segment) source)
        {
            var (request, segment) = source;

            return new OrderSeatV8DeleteSeatCommand
            {
                Pos = request.Pos,
                Parties = request.Parties,
                ChannelId = request.ChannelId,
                Rloc = request.Rloc,
                ExternalRloc = request.ExternalRloc,
                CustomerDetails = request.CustomerDetails,
                OrderAmendments = request.OrderAmendments.SelectMany(x => new List<OrderAmendment>
                {
                    new OrderAmendment
                    {
                        ProductType = x.ProductType,
                        CustomerReference = x.CustomerReference,
                        CustomerPrices = x.CustomerPrices,
                        FlightItems = x.FlightItems.Where(fi => fi.Segments.SegmentId == segment.SegmentId).ToList()                       
                    }
                }).ToList(),
                PaymentDetails = request.PaymentDetails,
                OriginDestinations = request.OriginDestinations,
                SessionId = request.SessionId,
                SeatTransactionId = request.SeatTransactionId.Where(st => st.SegmentId == segment.SegmentId).ToList()
            };

        }
           
    } 
}
