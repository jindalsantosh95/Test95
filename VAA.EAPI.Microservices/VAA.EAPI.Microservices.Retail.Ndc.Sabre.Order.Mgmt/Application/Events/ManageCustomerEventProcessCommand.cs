using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events
{
    public class ManageCustomerEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
