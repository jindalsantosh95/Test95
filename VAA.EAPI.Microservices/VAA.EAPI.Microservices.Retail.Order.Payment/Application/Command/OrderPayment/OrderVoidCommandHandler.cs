using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderVoidCommandHandler : IRequestHandler<OrderVoidCommand, Result<Response<Air4formsOfPaymentsResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderVoidCommandHandler> _logger;

        public OrderVoidCommandHandler(IMediator mediator, ILogger<OrderVoidCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<Air4formsOfPaymentsResponse>>> Handle(OrderVoidCommand request, CancellationToken cancellationToken)
        {
            try
            {

                return await _mediator.Send(new Air4VoidServiceRequest() { PaymentReferenceId = request.PaymentReferenceId }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return Result.Failure<Response<Air4formsOfPaymentsResponse>>((ex.InnerException ?? ex).ToString());
            }



        }
    }
}
