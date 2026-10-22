using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events
{
    public interface IUpdateTransactionPnrEventHandler
    {
        Task<Result<Response<string>>> Handle(UpdateTransactionPnrEvent updateTransactionPnrEvent, CancellationToken cancellationToken);
    }
}
