using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Interface
{
    public interface IFlightOrderNotificationDetails
    {
        Task GetFlightOrderDetailAsync(CloudEvent cloudEvent, CancellationToken cancellationToken);
    }
}
