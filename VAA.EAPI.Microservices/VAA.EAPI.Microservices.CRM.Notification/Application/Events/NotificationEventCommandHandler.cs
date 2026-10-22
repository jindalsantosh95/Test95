using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Notification.Constants;

namespace VAA.EAPI.Microservices.CRM.Notification.Application.Events
{
    public class NotificationEventCommandHandler : INotificationEventCommand
    {
        private readonly IAirshipService _airshipService;
        private readonly ILogger<NotificationEventCommandHandler> _logger;

        public NotificationEventCommandHandler(IAirshipService airshipService, ILogger<NotificationEventCommandHandler> logger)
        {
            _airshipService = airshipService ?? throw new ArgumentNullException(nameof(_airshipService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            try
            {
                var notificationRequest = Json.Deserialize<NotificationRequest>(Convert.FromBase64String(cloudEventReq.Data as string).DecompressToString());

                var requestData = notificationRequest.FlightOrderData;
                var airshipRequest = new AirshipRequest
                {
                    PnrsWithDate = requestData.Select(x => x.BookingReference?.PnrLocator + x.BookingReference?.CreatedDate.ToString("ddMMMyy").ToUpper()),
                    FlightNumber = requestData.First().EventFlight.OperatingCarrier.FlightNum,
                    DeepLink= notificationRequest.DeepLink,
                    Campaigns = notificationRequest.Campaigns,
                    Message= notificationRequest.Message,
                    CustomEventName = notificationRequest.CustomEventName
                };

                await _airshipService.Handle(airshipRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in NotificationEventCommandHandler: {ex.Message}");
            }
        }
    }
}

