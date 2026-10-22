using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentDetailsCommandHandler : IRequestHandler<OrderPaymentDetailsCommand, Result<Response<PaymentTransactionDetails>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaymentDetailsCommandHandler> _logger;

        public OrderPaymentDetailsCommandHandler(IMediator mediator, ILogger<OrderPaymentDetailsCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<PaymentTransactionDetails>>> Handle(OrderPaymentDetailsCommand request, CancellationToken cancellationToken)
        {
            Result<Response<PaymentTransactionDetails>> result = new();

            switch (request.PaymentType)
            {
                case PaymentType.APM:
                    result = await _mediator.Send(new Air4FormsOfPaymentsQuery() { Channel = request.Channel, PaymentReferenceId = request.PaymentReferenceId }, cancellationToken);
                    break;
                case PaymentType.Card:
                    result = await _mediator.Send(new Air4PaymentDetailsQuery() { Channel = request.Channel, PaymentReferenceId = request.PaymentReferenceId }, cancellationToken);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}