using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class AuthorizeCancelPaymentCommandHandler : IAuthorizeCancelPaymentCommand
    {
        private readonly ILogger<AuthorizePaymentCommandHandler> _logger;
        private readonly IPaymentAuthorizeCancelCommand _paymentAuthorizeCancelCommandHandler;

        public AuthorizeCancelPaymentCommandHandler(ILogger<AuthorizePaymentCommandHandler> logger, IPaymentAuthorizeCancelCommand authorizeCancelPaymentCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentAuthorizeCancelCommandHandler = authorizeCancelPaymentCommandHandler ?? throw new ArgumentNullException(nameof(authorizeCancelPaymentCommandHandler));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var PaymentResponse = new Domain.PaymentResponse();

                var authorizeCancelResponse = await _paymentAuthorizeCancelCommandHandler.Handle(new Infrastructure.Models.PaymentAuthorizeRequest() { TrackID = request.TrackId}, cancellationToken);

                PaymentResponse = PaymentResponseMapper.AuthorizeResponseMapper(authorizeCancelResponse.Value.Value);

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
