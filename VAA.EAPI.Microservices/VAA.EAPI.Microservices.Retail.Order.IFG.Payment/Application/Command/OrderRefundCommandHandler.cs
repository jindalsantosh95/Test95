using CSharpFunctionalExtensions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Application.Command
{
    public class OrderRefundCommandHandler : IOrderRefundCommand
    {
        private readonly IPaymentOrderRefundCommand _paymentOrderRefundCommandHandler;

        public OrderRefundCommandHandler( IPaymentOrderRefundCommand paymentOrderRefundCommandHandler)
        {
            _paymentOrderRefundCommandHandler = paymentOrderRefundCommandHandler ?? throw new ArgumentNullException(nameof(paymentOrderRefundCommandHandler));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var PaymentResponse = new Domain.PaymentResponse();

                var orderRefundRequest = OrderRefundMapper.OrderRefundRequest(request);
                var orderRefundResponse = await _paymentOrderRefundCommandHandler.Handle(orderRefundRequest, cancellationToken);

                PaymentResponse = PaymentResponseMapper.AuthorizeResponseMapper(orderRefundResponse.Value.Value);

                if (PaymentResponse.Errors == null)
                    return new Response<PaymentResponse>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = PaymentResponse };
                else
                    return new Response<PaymentResponse>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest, Errors = PaymentResponse.Errors.ToList() };
            }

            catch (Exception ex)
            {
                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.DefaultTechErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                return Result.Failure<Response<PaymentResponse>>(Json.Serialize(errors));
            }

        }
    }
}
