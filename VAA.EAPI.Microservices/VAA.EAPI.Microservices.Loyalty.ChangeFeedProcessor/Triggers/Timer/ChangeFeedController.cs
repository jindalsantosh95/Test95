using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command.PhoneUpdate;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Triggers.Timer
{
    public class ChangeFeedController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChangeFeedController> _logger;

        public ChangeFeedController(IMediator mediator, ILogger<ChangeFeedController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CosmosChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "%CustDB:SCVCustSearchDBName%",
            containerName: Constants.Application.CustomerCollection,
            Connection =  Constants.Application.CosmosConnectionString,
            LeaseContainerName = Constants.Application.LeaseCollectionName,
            CreateLeaseContainerIfNotExists = true,
            MaxItemsPerInvocation = Constants.Application.MaxItemsPerInvocation
            )]IReadOnlyList<object> documents, ILogger log)
        {
            foreach (var doc in documents)
            {
                if (doc != null)
                {
                    var baseDoc = Json.Deserialize<DocIdentifier>(doc.ToString());

                    try
                    {
                        switch (baseDoc?.DocType)
                        {
                            case "CustomerSearch":
                                await _mediator.Send(new ChangeFeedProcesCommand(Json.Deserialize<CustomerSearch>(doc.ToString())));
                                break;
                            case "Customer":
                                await _mediator.Send(new CustomerSearchUpdateCommand(Json.Deserialize<Customer>(doc.ToString())));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in handling customer collection change feed");
                        throw;
                    }
                }
            }
        }
    }
}
