using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class FCNumerToPNRResponseEventCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
