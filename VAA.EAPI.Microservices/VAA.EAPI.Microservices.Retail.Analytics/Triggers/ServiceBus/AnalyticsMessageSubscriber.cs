using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Analytics.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Analytics.Triggers.ServiceBus
{
    public sealed class AnalyticsMessageSubscriber
    {
        private readonly IMediator _mediator;

        public AnalyticsMessageSubscriber(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [Function("AnalyticsMessageSubscriber")]
        public async Task AnalyticsMessageSubscription([ServiceBusTrigger(Infrastructure.Constants.Constants.AnalyticsServiceBusTopicName, Infrastructure.Constants.Constants.AnalyticsServiceBusSubscriptionName, Connection = Infrastructure.Constants.Constants.AnalyticsServiceBusConnectionString)] string request, CancellationToken cancellationToken)
        {
            var trackAnalyticsMessage = (TrackAnalyticsMessage)request;

            AsyncContext.Current.MapFromCloudEvent(trackAnalyticsMessage);

            var trackAnalyticsMessageCommand = (TrackAnalyticsMessageCommand)trackAnalyticsMessage;

            await _mediator.Send(trackAnalyticsMessageCommand, cancellationToken);
        }
    }
}
