using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public interface ISvcOrderRepository
    {
        Task<IReadOnlyCollection<OrderDoc>> GetByPnrAsync(string pnr, CancellationToken cancellationToken);
    }
}
