using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership
{
    public class PartnerMembershipQuery : TransactionBase, IRequest<Result<Response<PartnerMembershipResponse>>>
    {
    }
}
