using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.RefundUpdate
{
    public class RefundUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
