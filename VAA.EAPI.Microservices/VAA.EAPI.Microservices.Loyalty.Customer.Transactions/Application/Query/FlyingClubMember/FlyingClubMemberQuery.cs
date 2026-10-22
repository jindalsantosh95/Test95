using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember
{
    public class FlyingClubMemberQuery : TransactionBase, IRequest<Result<Response<FlyingClubMemberResponse>>>
    { }
}
