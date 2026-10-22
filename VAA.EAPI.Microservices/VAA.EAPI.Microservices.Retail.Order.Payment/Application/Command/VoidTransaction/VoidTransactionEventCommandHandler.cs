using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.RefundTicket;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.VoidTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction
{
    public class VoidTransactionEventCommandHandler : IRequestHandler<VoidTransactionEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VoidTransactionEventCommandHandler> _logger;
        public VoidTransactionEventCommandHandler(IMediator mediator, ILogger<VoidTransactionEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(VoidTransactionEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"VoidTransactionEventCommandHandler | Correlation ID : {request.Id}");
            CloudEvent eventTobePublished;
            try
            {
                //Map to Void transaction request
                var voidOrderDetailsData = Json.Deserialize<VoidTransactionDetails>(Convert.ToString(request.Data));
                //Call CellPoint Digital API                
                VoidTransactionRequest voidTransactionRequest = new(); //Will be replaced by MapToCPDAPIReqeust Mapper

                var refundApiCallSucceeded = await _mediator.Send
                    (
                        request: new RefundAPIRepoCommand()
                        {
                            VoidTransactionRequest = voidTransactionRequest
                        },
                        cancellationToken: cancellationToken
                    );

                _logger.LogDebug($"VoidTransactionEventCommandHandler | refundApiCall Status {refundApiCallSucceeded}");
                if (refundApiCallSucceeded)
                {
                    //Map VoidTransaction event to Void Order Update request
                    var voidOrderUpdateRequest = VoidOrderDataMapper.MapToVoidOrderData(voidOrderDetailsData);
                    //Create Cloud event
                    eventTobePublished = VoidTransactionMapper.MapToDataEvent(request, voidOrderUpdateRequest);
                }
                else
                {
                    //Map VoidTransaction event to Email MS request

                    ///////////////////////////////////////////////////////////////
                    /// <PlaceHolder>For creating Email MS request </PlaceHolder>
                    ///////////////////////////////////////////////////////////////

                    //Create Cloud event
                    eventTobePublished = CPDBusinessErrorMapper.MapToEmailEvent(request);

                }
                //Store cloud event in event store

                try
                {
                    _logger.LogDebug($"VoidTransactionEventCommandHandler | event store start");
                    await _mediator.Send
                                    (
                                         request: new EventRequestInsertCommand
                                         {
                                             Id = eventTobePublished.Id,
                                             Type = eventTobePublished.Type,
                                             Data = eventTobePublished.Data,
                                             Subject = eventTobePublished.Subject
                                         },
                                         cancellationToken: cancellationToken
                                     );
                    _logger.LogDebug($"VoidTransactionEventCommandHandler | event store end");
                    //Publish cloud event to update Cosmos DB with Refund status
                    _logger.LogDebug($"VoidTransactionEventCommandHandler | publish event start");
                    await _mediator.Publish(eventTobePublished, cancellationToken);
                    _logger.LogDebug($"VoidTransactionEventCommandHandler | publish event end");
                }
                catch (System.Exception)
                {
                    _logger.LogError($"Rollback from event store for CorrelationId: {request.Id} due to error in VoidTransactionEventCommandHandler");

                    //Rollback event from event store
                    await _mediator.Send
                        (
                            request: new EventStoreRequestDeleteCommand
                            {
                                Id = eventTobePublished.Id,
                                Type = eventTobePublished.Type,
                                Data = eventTobePublished.Data,
                                Subject = eventTobePublished.Subject
                            },
                            cancellationToken: cancellationToken
                        );
                    var error = new Response<string>()
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.ServiceBusError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    }; throw new MessageBrokerException($"{Json.Serialize(error.Errors)}");
                }

            }
            catch (MessageBrokerException ex)
            {
                _logger.LogError(ex.Message); throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Exception in VoidTransactionEventCommandHandler for CorrelationId: {request.Id}"); throw;
            }
            return Unit.Value;
        }
    }
}
