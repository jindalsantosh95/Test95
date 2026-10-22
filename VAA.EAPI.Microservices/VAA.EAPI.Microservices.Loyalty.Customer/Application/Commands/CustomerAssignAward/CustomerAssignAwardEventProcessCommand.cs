using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class CustomerAssignAwardEventProcessCommand : CloudEvent, IRequest<bool>
    {
    }
}
