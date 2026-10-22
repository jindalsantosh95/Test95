using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public interface INDCOrderHistoryUpdateCommand
    {
        Task<Result<Unit>> Handle(CloudEvent cloudEvent);
    }
}