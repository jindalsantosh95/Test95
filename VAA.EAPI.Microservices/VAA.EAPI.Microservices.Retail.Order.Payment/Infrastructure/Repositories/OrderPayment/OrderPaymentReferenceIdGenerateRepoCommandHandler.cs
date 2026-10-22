using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateRepoCommandHandler : IRequestHandler<OrderPaymentReferenceIdGenerateRepoCommand, Result<Response<string>>>
    {
        private readonly ILogger<OrderPaymentReferenceIdGenerateRepoCommandHandler> _logger;
        private readonly IMediator _mediator;

        public OrderPaymentReferenceIdGenerateRepoCommandHandler(IMediator mediator, ILogger<OrderPaymentReferenceIdGenerateRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(OrderPaymentReferenceIdGenerateRepoCommand request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new OrderPaymentReferenceIdGenerateServiceCommand() { Channel = request.Channel, PaymentRequest = request.Request }, cancellationToken);
        }
    }
}
