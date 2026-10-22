using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount
{
    public class UpdateAzureB2CDetailsCommand : CloudEvent, IRequest<Result<bool>>
    {
    }
}
