using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageCustomerEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
