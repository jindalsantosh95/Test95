using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability
{
    public class SeatFulfilmentResponseQuery : IRequest<Result<Response<SeatFulfilmentResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
