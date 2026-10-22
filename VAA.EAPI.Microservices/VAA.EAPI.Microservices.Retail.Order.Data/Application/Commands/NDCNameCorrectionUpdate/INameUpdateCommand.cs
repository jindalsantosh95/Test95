using CSharpFunctionalExtensions;
using MediatR;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCNameCorrectionUpdate
{
    public interface INameUpdateCommand
    {
        Task<Result<Unit>> Handle(CloudEvent cloudEvent);
    }
}