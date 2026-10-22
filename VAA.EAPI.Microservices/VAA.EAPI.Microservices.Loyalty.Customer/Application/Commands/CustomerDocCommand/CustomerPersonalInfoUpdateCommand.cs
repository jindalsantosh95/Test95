using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand
{
    public class CustomerPersonalInfoUpdateCommand : CustomerPersonalInfoUpdateRequest, IRequest<Result<Response<string>>>
    {

    }
}


