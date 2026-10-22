using MediatR;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatV8AddSeatQuery : SeatFulfilmentRequest, IRequest<IEnumerable<AddResponseType>>
    {
        public static explicit operator SeatV8AddSeatQuery((SeatV8AddSeatQuery request, string segment) source)
        {
            var (request, segment) = source;

            return new SeatV8AddSeatQuery
            {
                ChannelId = request.ChannelId,
                Pos = request.Pos,
                BookingReferences = request.BookingReferences,
                Parties = request.Parties,
                SelectedSeatInfo = request.SelectedSeatInfo.Where(x => x.FlightIdRef == segment),
                paymentDetails = request.paymentDetails,
                SeatTransactionIds = request.SeatTransactionIds.Where(x => x.SegmentId == segment)
            };

        }
    }
}
