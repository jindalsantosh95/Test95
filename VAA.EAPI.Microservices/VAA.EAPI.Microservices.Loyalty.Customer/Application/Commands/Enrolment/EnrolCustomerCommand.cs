using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class EnrolCustomerCommand : CreateCustomerEntities, IRequest<bool>
    {

    }
}
