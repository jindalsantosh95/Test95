using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageSpecialServiceCommand : OrderChangeRequestData, IRequest<Result<Response<string>>>
    {
    }
}
