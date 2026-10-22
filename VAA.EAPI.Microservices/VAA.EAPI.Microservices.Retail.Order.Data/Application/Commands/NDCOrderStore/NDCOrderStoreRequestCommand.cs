using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public class NDCOrderStoreRequestCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
