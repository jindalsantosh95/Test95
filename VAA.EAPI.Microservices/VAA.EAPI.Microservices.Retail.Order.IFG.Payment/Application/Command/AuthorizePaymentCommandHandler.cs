using CSharpFunctionalExtensions;
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
    public class AuthorizePaymentCommandHandler : IAuthorizePaymentCommand
    {
        private readonly ILogger<AuthorizePaymentCommandHandler> _logger;
        private readonly IPaymentAuthorizeCommand _paymentAuthorizeCommandHandler;

        public AuthorizePaymentCommandHandler(ILogger<AuthorizePaymentCommandHandler> logger, IPaymentAuthorizeCommand paymentAuthorizeCommandHandler) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentAuthorizeCommandHandler = paymentAuthorizeCommandHandler ?? throw new ArgumentNullException(nameof(paymentAuthorizeCommandHandler));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var PaymentResponse = new Domain.PaymentResponse();
                
                var authorizationRequest = AuthorizePaymentMapper.AuthorizePaymentRequest(request);
                var authorizationResponse = await _paymentAuthorizeCommandHandler.Handle(authorizationRequest, cancellationToken);

                PaymentResponse = PaymentResponseMapper.AuthorizeResponseMapper(authorizationResponse.Value.Value);

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


