using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class LoyaltyCustomerAssignAwardCommand : AssignAwardRequestData, IRequest<Result<Response<string>>>
    {

    }
}
