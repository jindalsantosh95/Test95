using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile
{
    public class CustomerProfileQuery : TransactionBase, IRequest<Result<Response<CustomerProfileDocResponse>>>
    {
    }
}