using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain.Enum;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Triggers.ChangeFeed
{
    public class ChangeFeedController
    {
        private readonly IMediator _mediator;

        public ChangeFeedController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CosmosDBChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(databaseName: "%OrderDB:SCVOrderDBName%", containerName: "%OrderDB:SCVOrderDBCollection%", LeaseContainerPrefix = "%LeaseContainerPrefix%", Connection = Constants.Application.CosmosConnectionString, LeaseContainerName = "%LeaseContainerName%", CreateLeaseContainerIfNotExists = true, MaxItemsPerInvocation = 10)] IReadOnlyList<OrderDocument> documents, ILogger log)
        {
            foreach (var doc in documents)
            {
                if (doc != null && doc.DocType == DocumentType.Order.ToString())
                {
                    await _mediator.Send(new ChangeFeedOrderProcessCommand(doc));
                }
            }
        }
    }
}
