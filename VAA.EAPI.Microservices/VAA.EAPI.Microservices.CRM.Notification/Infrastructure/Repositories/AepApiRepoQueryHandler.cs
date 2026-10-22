using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Repositories
{
    public class AepApiRepoQueryHandler : IAepApiRepoQuery
    {
        private readonly ILogger<AepApiRepoQueryHandler> _logger;

        public AepApiRepoQueryHandler(ILogger<AepApiRepoQueryHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(List<FlightOrderData> notificationRequest, CancellationToken cancellationToken)
        {
            var passengerWiseOrders = notificationRequest.SelectMany(flightOrder => flightOrder.Passengers, (flightOrder, passenger) => new AepNotificationRequest()
            {
                BookingReference = flightOrder.BookingReference,
                Journeys = flightOrder.Journeys,
                passenger = new Passenger
                {
                    AmperityId = passenger.AmperityId,
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName,
                    PaxType = passenger.PaxType
                }
            });

            foreach (var order in passengerWiseOrders)
            {
                //To do AEP API call
            }
        }
    }
}
