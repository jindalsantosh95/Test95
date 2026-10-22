using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentCaptureCommandHandler : IRequestHandler<OrderPaymentCaptureCommand, Result<Response<PaymentTransactionDetails>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentCaptureCommandHandler> _logger;

        public OrderPaymentCaptureCommandHandler(IMediator mediator, ILogger<OrderPaymentCaptureCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<PaymentTransactionDetails>>> Handle(OrderPaymentCaptureCommand orderPaymentCaptureCommand, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new Air4PaymentCaptureCommand()
            {
                PaymentReferenceId = orderPaymentCaptureCommand.Request.PaymentReferenceId,
                PaymentType = orderPaymentCaptureCommand.Request.PaymentType,
                VendorId = orderPaymentCaptureCommand.Request.VendorId,
                RLOC = orderPaymentCaptureCommand.Request.RLOC,
                TicketNumber = orderPaymentCaptureCommand.Request.TicketNumber,
            }, cancellationToken);
        }
    }
}
