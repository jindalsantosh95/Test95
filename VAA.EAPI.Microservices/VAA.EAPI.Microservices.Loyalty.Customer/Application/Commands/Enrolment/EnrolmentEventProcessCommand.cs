using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class EnrolmentEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }

}
