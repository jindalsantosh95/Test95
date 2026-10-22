using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderStore
{
    public class InsuranceStatusUpdateCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
