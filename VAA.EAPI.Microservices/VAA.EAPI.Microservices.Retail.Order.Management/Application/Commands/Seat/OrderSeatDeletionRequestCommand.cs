using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.Seat
{
    public class OrderSeatDeletionRequestCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }

}
