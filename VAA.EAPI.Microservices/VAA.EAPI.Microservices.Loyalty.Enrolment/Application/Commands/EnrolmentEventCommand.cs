using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands
{
    public class EnrolmentEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
