using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards
{
    public class AwardsQuery : TransactionBase, IRequest<Result<Response<AwardResponse>>>
    {

    }
}
