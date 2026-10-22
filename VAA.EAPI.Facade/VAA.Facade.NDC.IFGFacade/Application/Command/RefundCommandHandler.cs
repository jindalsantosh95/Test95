using CSharpFunctionalExtensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Mapper;
using VAA.Facade.NDC.IFGFacade.Validations;

namespace VAA.Facade.NDC.IFGFacade.Application.Command
{
    public class RefundCommandHandler(IPaymentMSRefundCommand paymentMSRefundCommand, Microsoft.Extensions.Logging.ILogger<RefundCommandHandler> logger = null) : IRefundCommand
    {
        private readonly IPaymentMSRefundCommand _paymentMSRefundCommand = paymentMSRefundCommand;
        private readonly Microsoft.Extensions.Logging.ILogger<RefundCommandHandler> _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RefundCommandHandler>.Instance;
        public async Task<Result<Response<PaymentResponse>>> Handle(RefundRequest refundRequest, CancellationToken cancellationToken)
        {
            try
            {
            var validationResult = PaymentRequestValidations.ValidateRefundRequest(refundRequest);

            if (validationResult.Any())
            {
                return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. validationResult] };
            }

            var refundPaymentMSRequest = (RefundPaymentRequest)refundRequest;
            var paymentMSResponse = await _paymentMSRefundCommand.Handle(refundPaymentMSRequest);

            var paymentResponse = PaymentResponseMapper.MapPaymentResponse(paymentMSResponse, false);

            if (paymentResponse.Errors == null)
                return new Response<PaymentResponse>() { Status = Status.OK, Value = paymentResponse };

            return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. paymentResponse.Errors] };

            }
            catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (System.Exception exception)
            {
                if (_logger != null)
                    _logger.Log(
                        Microsoft.Extensions.Logging.LogLevel.Error,
                        default,
                        "Refund command failed.",
                        exception,
                        (state, ex) => state);

                return new Response<PaymentResponse>() { Status = Status.Error, Errors = [new() { Message = "Payment refund failed." }] };
            }
        }
    }
}
