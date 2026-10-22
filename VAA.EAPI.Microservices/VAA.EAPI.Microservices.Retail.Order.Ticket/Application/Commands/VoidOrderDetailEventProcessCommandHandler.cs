using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicketOrder;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands
{
    public class VoidOrderDetailEventProcessCommandHandler : IRequestHandler<VoidOrderDetailEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VoidOrderDetailEventProcessCommandHandler> _logger;
        public VoidOrderDetailEventProcessCommandHandler(IMediator mediator, ILogger<VoidOrderDetailEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(VoidOrderDetailEventProcessCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"VoidOrderDetailEventProcessCommandHandler | CorrelationId : {request.Id}");
            try
            {
                //Map to VoidOrderDetailRequest
                var voidOrderDetailsData = Json.Deserialize<VoidOrderDetails>(Convert.ToString(request.Data));

                //Validate if Order is already marked as RiskFreeCancelled
                if (isOrderStatusRFC(voidOrderDetailsData))
                {
                    _logger.LogWarning($"VoidOrderDetailEventProcessCommandHandler | Order is already marked as RiskFreeCancelled | CorrelationId : {request.Id}");
                    return Unit.Value;
                }

                //RFC Logic
                if (ValidateRiskFreeCancellation(voidOrderDetailsData))
                {
                    //Map to Order Refund event request
                    var orderRefundEvent = OrderRefundEventMapper.MapToOrderRefundEvent(request);
                    //Store event in to event store
                    _logger.LogDebug("VoidOrderDetailEventProcessCommandHandler | EventStore - Start");
                    await _mediator.Send
                        (
                             request: new EventRequestInsertCommand
                             {
                                 Id = orderRefundEvent.Id,
                                 Type = orderRefundEvent.Type,
                                 Data = orderRefundEvent.Data,
                                 Subject = orderRefundEvent.Subject
                             },
                             cancellationToken: cancellationToken
                         );
                    _logger.LogDebug("VoidOrderDetailEventProcessCommandHandler | EventStore - End");
                    //Publish event to Payment Service bus Topic to initiate refund
                    try
                    {
                        _logger.LogDebug($"VoidOrderDetailEventProcessCommandHandler |publish event start");
                        await _mediator.Publish(orderRefundEvent, CancellationToken.None);
                        _logger.LogDebug($"VoidOrderDetailEventProcessCommandHandler |publish event end");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Rollback from event store for CorrelationId: {request.Id} due to error in VoidOrderDetailEventProcessCommandHandler");
                        //Rollback event from event store
                        await _mediator.Send
                        (
                            request: new EventStoreRequestDeleteCommand
                            {
                                Id = orderRefundEvent.Id,
                                Type = orderRefundEvent.Type,
                                Data = orderRefundEvent.Data,
                                Subject = orderRefundEvent.Subject
                            },
                            cancellationToken: cancellationToken
                        );

                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning($"VoidOrderDetailEventProcessCommandHandler | Not a risk free cancellation | CorrelationId : {request.Id}");
                }
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in VoidOrderDetailEventProcessCommandHandler for CorrelationId: {request.Id}");
                throw;
            }
        }

        public bool isOrderStatusRFC(VoidOrderDetails voidOrderDetailsData)
        {
            return voidOrderDetailsData?.OrderData?.OrderStatus == "RiskFreeCancelled";
        }

        public bool ValidateRiskFreeCancellation(VoidOrderDetails request)
        {
            //Actual RFC logic param would  be decided later
            _logger.LogDebug("VoidOrderDetailEventProcessCommandHandler | RFC Logic |Check point");

            if (request.Tickets != null && request.Tickets.FirstOrDefault().LastTransactionEventGTs != null)
            {
                var lastTransactionDate = DateTime.ParseExact(request.Tickets.FirstOrDefault().LastTransactionEventGTs, Constants.Application.LastTransactionDateFormat, CultureInfo.InvariantCulture);
                return (DateTime.UtcNow - lastTransactionDate).TotalHours <= ConfigReader.Get<int>(Constants.Environment.RefundWindowInHours);
            }
            else return false;
        }
    }
}
