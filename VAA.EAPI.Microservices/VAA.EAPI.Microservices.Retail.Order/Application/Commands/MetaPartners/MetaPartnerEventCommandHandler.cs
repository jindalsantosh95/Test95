using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public sealed class MetaPartnerEventCommandHandler(IServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ILogger<MetaPartnerEventCommandHandler> logger) : IMetaPartnerEventCommandHandler
    {
        private readonly IServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        private readonly ILogger<MetaPartnerEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> Handle(MetaPartnerEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request is null) return false;
                if (request.Source.IsNullOrWhiteSpace()) return false;

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = request,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = request.Source
                }, cancellationToken);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Meta partner event command handler failed:\n\n{string.Join(Environment.NewLine, ((MetaPartnerCommand)request).Attributes.Select(x => $"{x.Key}={x.Value}{Environment.NewLine}"))}");

                return false;
            }
        }
    }
}
