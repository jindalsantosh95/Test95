using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public interface IScvBookingRepository
    {
        Task<IReadOnlyCollection<string>> GetOrderIdsByPnrAsync(string pnr, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<string>> GetMatchingOrderIdsAsync(string orderId, string flightDate, string firstName, string lastName, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<OrderDoc>> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken);
    }
}
