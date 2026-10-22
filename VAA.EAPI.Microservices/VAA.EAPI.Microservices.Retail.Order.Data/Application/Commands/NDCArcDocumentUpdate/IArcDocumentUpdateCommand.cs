using CSharpFunctionalExtensions;
using MediatR;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCArcDocumentUpdate
{
    public interface IArcDocumentUpdateCommand
    {
        Task<Result> Handle(CloudEvent cloudEvent);
    }
}
