using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class ActionExecutionEventCommand : CloudEvent, IRequest<Result<bool>>
    {
    }
}
