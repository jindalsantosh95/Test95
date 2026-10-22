using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VacEventEnrolmentCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
