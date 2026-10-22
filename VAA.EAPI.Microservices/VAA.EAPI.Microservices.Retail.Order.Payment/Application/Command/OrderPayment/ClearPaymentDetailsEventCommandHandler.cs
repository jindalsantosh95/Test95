using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.InsurancePurchase;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class ClearPaymentDetailsEventCommandHandler : IRequestHandler<ClearPaymentDetailsEventCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClearPaymentDetailsEventCommandHandler> _logger;


        public ClearPaymentDetailsEventCommandHandler(IMediator mediator, ILogger<ClearPaymentDetailsEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(ClearPaymentDetailsEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var paymentDetails = Json.Deserialize<PaymentRequest>(Convert.ToString(request.Data));

                var response = await _mediator.Send(new ClearPaymentDetailsServiceCommand { PaymentReferenceId = paymentDetails.PaymentReferenceId }, cancellationToken);

                return Result.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Unit>(ex.Message);
            }

        }
    }
}