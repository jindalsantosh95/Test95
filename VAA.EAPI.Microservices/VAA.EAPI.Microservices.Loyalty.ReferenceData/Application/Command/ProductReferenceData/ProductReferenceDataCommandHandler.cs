using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.ProductReferenceData;

public sealed class ProductReferenceDataCommandHandler : IProductReferenceDataCommandHandler
{
    private readonly IProductDataQueryHandler _productDataQueryHandler;
    private readonly ILogger<ProductReferenceDataCommandHandler> _logger;
    private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
    private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
    private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;

    public ProductReferenceDataCommandHandler(IProductDataQueryHandler productDataQueryHandler, ILogger<ProductReferenceDataCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
    {
        _productDataQueryHandler = productDataQueryHandler ?? throw new ArgumentNullException(nameof(productDataQueryHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
    }

    public async Task<Result<string>> Handle(ProductReferenceDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<ProductDetails> productDetails;
            var productsResponse = await _productDataQueryHandler.Handle(new ProductDataQuery(), cancellationToken);

            if (productsResponse.IsFailure)
                return Result.Failure<string>("Failed to retrieve Product Data from sql DB");

            productDetails = productsResponse.Value?.Value;

            if (productDetails is null)
                return Result.Failure<string>("Product response does not contain product details");

            CloudEvent cloudEvent = EventMapper.MapProductsToCloudEvent(new ProductsEventData { Products = productDetails.Select(x => (Product)x) });

            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = productDetails.Select(x => x.RowId), Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = productDetails.Select(x => x.RowId), Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }

            return Result.Success<string>(Constants.Application.EventSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred in ProductReferenceDataCommandHandler: {ex.Message}");
            return Result.Failure<string>(ex.Message.ToString());
        }
    }
}