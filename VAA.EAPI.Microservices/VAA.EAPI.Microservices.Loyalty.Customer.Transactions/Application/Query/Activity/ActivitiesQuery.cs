using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Activity
{
    public class ActivitiesQuery : TransactionBase, IRequest<Result<Response<ActivitiesResponse>>>
    {
    }
}
