using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;

namespace VAA.EAPI.Infrastructure.Services.Analytics
{
    public sealed class TrackAnalyticsEventCommandHandler : ITrackAnalyticsEventCommandHandler
    {
        private readonly IServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly ILogger<TrackAnalyticsEventCommandHandler> _logger;

        public TrackAnalyticsEventCommandHandler(IServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ILogger<TrackAnalyticsEventCommandHandler> logger)
        {
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(TrackAnalyticsEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null) return false;
                if (request.ServiceBusConnectionString.IsNullOrWhiteSpace()) return false;
                if (request.Source.IsNullOrWhiteSpace()) return false;

                _ = _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    ServiceBusConnectionString = request.ServiceBusConnectionString,
                    ServiceBusTopicName = request.Source,
                    Message = request
                }, cancellationToken);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Track Analytics Event Command Handler failed ");
                return false;
            }
        }
    }
}