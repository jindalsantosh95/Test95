
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.PassengerRetrieval
{
    public interface IPassengerRetrieval
    {
        public Task GetPassengerFlightDetailAsync(CloudEvent request, CancellationToken cancellationToken);
    }

}
