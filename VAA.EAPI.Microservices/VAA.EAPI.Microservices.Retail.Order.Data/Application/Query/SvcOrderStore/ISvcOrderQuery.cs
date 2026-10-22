using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public interface ISvcOrderQuery
    {
        Task<Response<SvcOrderValidationResponse>> HandleAsync(string pnr, string firstName, string lastName, CancellationToken cancellationToken);
    }
}
