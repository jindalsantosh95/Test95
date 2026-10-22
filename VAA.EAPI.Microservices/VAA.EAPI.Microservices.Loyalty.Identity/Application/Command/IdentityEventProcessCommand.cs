using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

public class IdentityEventProcessCommand : CloudEvent, IRequest<Result<bool>>
{

}