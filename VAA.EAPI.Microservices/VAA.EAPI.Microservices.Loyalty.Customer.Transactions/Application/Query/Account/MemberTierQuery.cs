
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Account;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Account
{
    public class MemberTierQuery : TransactionBase, IRequest<Result<Response<MemberTierResponse>>>
    {
    }
}
