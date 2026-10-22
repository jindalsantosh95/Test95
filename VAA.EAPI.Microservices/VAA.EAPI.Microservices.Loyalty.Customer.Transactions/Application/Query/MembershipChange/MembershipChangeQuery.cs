using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MembershipChange
{
    public class MembershipChangeQuery : TransactionBase, IRequest<Result<Response<MembershipChangeResponse>>>
    { }
}

