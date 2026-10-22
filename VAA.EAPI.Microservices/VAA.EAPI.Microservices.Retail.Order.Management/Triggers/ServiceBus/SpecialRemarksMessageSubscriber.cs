using Microsoft.Azure.Functions.Worker;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Enums;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Services;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class SpecialRemarksMessageSubscriber
    {
        private readonly ISpecialRemarksService _specialRemarksService;

        public SpecialRemarksMessageSubscriber(ISpecialRemarksService specialRemarksService)
        {
            _specialRemarksService = specialRemarksService;
        }

        [Function("SpecialRemarksMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeEventHandlerTopicName, Constants.Application.AddSpecialRemarksEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            var request = Json.Deserialize<AddSpecialRemarkRequest>(cloudEvent.Data.ToString());
            var channelId = AsyncContext.Current?.Channel?.Any() == true
                ? AsyncContext.Current.GetDeltaChannel()
                : nameof(ChannelType.DFWEB);

            var result = await _specialRemarksService.AddAsync(request, channelId, cancellationToken);

            if (result.Value?.Status != VAA.EAPI.Domain.Entities.Common.Status.OK)
            {
                throw new InvalidOperationException($"Special remarks event '{cloudEvent.Id}' was not processed successfully.");
            }
        }
    }
}
