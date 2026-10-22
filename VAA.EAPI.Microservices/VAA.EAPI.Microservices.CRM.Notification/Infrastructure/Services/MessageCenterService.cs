using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.AircraftChangeNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services
{
    public class MessageCenterService : IMessageCenterService
    {
        private readonly ILogger<MessageCenterService> _logger;
        private readonly IACMClient _client;

        public MessageCenterService(ILogger<MessageCenterService> logger, IACMClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task Handle(AircraftChangeRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var acmRequest = AircraftChangeRequestMapper.MapRequest(request);

                var retry = ConfigReader.Get<int>(Constants.Environment.RetryCounter);
                bool isSuccess = false;

                for (int i = 0; i < retry; i++)
                {
                    if (await _client.SendEmail(acmRequest))
                    {
                        isSuccess = true;
                        break;
                    }

                    if (i < retry - 1)
                    {
                        await Task.Delay(ConfigReader.Get<TimeSpan>(Constants.Environment.DelayTime), cancellationToken);
                    }
                }

                if (!isSuccess)
                {
                    _logger.LogError("Error occurred while sending Aircraft Change email for booking reference {BookingReference}", request.BookingReference);
                }            
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error in MessageCenterService: {ex.Message}");
            }
}

    }
}
