using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query
{
    public interface ISplitPnrResponseQuery
    {
        Task<Result<Response<OrderDocument>>> Handle(string CorrelationId, string Type, CancellationToken cancellationToken);
    }
}
