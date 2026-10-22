using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification.Interface
{
    public interface IFlightOrderAssociationService
    {
        Task<IEnumerable<string>> GetOrderIdsAsync(DateTime flightDate, string flightNumber, CancellationToken cancellationToken);
    }
}
