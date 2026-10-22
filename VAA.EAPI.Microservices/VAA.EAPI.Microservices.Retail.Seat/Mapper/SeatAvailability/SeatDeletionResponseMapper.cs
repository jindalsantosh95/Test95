using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using Error = VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability.Error;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatDeletionResponseMapper
    {
        public static SeatDeletionResponse MapDeleteResponse(List<DeleteResponseType> response , OrderChangeRequestData request)
        {
            var errors = response.Select(a => a.SeatRequestError?.ToList());
            return new SeatDeletionResponse()
            {
                OrderItems = request.OrderAmendments.SelectMany(x => x.FlightItems).Where(x => x.ActionContext == FlightItems.ActionContexts.Delete).Select(f =>
                {
                    return new OrderItems()
                    {
                        SeatNumber = f.CurrentSeatAssignment,
                        FlightIdRef = f.Segments.SegmentId
                    };
                }),
                Error = errors?.SelectMany(e => e).Select(s => new Error() { Message = s.Error.ErrorText }),
                OrderId = $"VS932{request.Rloc}{DateTime.UtcNow:yy}",
                Deletion = response.Any(r => r.SeatRequestSuccess != null)
            };
        }
    }
}
