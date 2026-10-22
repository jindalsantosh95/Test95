using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class RpaNdthEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
