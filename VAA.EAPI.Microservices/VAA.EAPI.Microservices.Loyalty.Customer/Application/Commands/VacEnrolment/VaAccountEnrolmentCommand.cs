using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VaAccountEnrolmentCommand : CloudEvent, IRequest<Result<Unit>>
    {
       
    }
}
