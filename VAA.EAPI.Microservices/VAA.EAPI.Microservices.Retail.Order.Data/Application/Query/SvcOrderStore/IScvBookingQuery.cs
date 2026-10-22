using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public interface IScvBookingQuery
    {
        Task<Response<IReadOnlyCollection<TripDetailsResponse>>> HandleAsync(string pnr, string flightDate, string firstName, string lastName, CancellationToken cancellationToken);
    }
}
