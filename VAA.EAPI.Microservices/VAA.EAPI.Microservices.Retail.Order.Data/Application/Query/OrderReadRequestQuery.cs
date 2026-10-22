using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query
{
    public class OrderReadRequestQuery : CloudEvent, IRequest<Result<Unit>>
    {

    }
}
