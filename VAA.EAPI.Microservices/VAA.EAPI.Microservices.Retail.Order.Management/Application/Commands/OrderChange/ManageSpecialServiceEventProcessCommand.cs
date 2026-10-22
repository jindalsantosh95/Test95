using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageSpecialServiceEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }
}
