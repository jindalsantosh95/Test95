using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG
{
    public interface ICreatePaymentDocumentEvent
    {
        Task<Result<Unit>> Handle(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}
