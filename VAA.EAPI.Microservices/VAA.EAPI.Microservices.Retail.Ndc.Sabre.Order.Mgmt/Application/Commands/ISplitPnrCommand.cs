using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public interface ISplitPnrCommand
    {
        Task<Result<Response<string>>> Handle(SplitPnrRequest splitPnrRequest, CancellationToken cancellationToken);
    }
}