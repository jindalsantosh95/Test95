using CSharpFunctionalExtensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Mapper.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;
using VAA.Facade.NDC.IFGFacade.Mapper;
using VAA.Facade.NDC.IFGFacade.Validations;

namespace VAA.Facade.NDC.IFGFacade.Application.Command
{
    public class AuthorizeCommandHandler(IPaymentMSAuthorizeCommand paymentMSAuthorizeCommand, IAirportDetailsQuery airportDetailsQuery, ICurrencyDetailsQuery currencyDetailsQuery, Microsoft.Extensions.Logging.ILogger<AuthorizeCommandHandler> logger = null) : IAuthorizeCommand
    {
        private readonly IPaymentMSAuthorizeCommand _paymentMSAuthorizeCommand = paymentMSAuthorizeCommand;
        private readonly IAirportDetailsQuery _airportDetailsQuery = airportDetailsQuery;
        private readonly ICurrencyDetailsQuery _currencyDetailsQuery = currencyDetailsQuery;
        private readonly Microsoft.Extensions.Logging.ILogger<AuthorizeCommandHandler> _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthorizeCommandHandler>.Instance;

        public async Task<Result<Response<PaymentResponse>>> Handle(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
        {
            try
            {
            var validationResult = PaymentRequestValidations.ValidateAuthRequest(authorizeRequest);

            if (validationResult.Any())
            {
                return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. validationResult] };
            }

            var airportDetails = await _airportDetailsQuery.Handle();

            var currencyDetails = await _currencyDetailsQuery.Handle();

            var authorizePaymentMSRequest = PaymentRequestMapper.MapAuthorizeRequest(authorizeRequest, airportDetails, currencyDetails); 
            var paymentMSResponse = await _paymentMSAuthorizeCommand.Handle(authorizePaymentMSRequest);

            var paymentResponse = PaymentResponseMapper.MapPaymentResponse(paymentMSResponse, true);

            if (paymentResponse.Errors == null)
                return new Response<PaymentResponse>() { Status = Status.OK, Value = paymentResponse };
            else
                return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. paymentResponse.Errors] };
            }
            catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (System.Exception exception)
            {
                if (_logger != null)
                {
                    _logger.LogError(
                        new Microsoft.Extensions.Logging.EventId(0, nameof(AuthorizeCommandHandler)),
                        exception,
                        "Authorize command failed.");
                }

                return new Response<PaymentResponse>() { Status = Status.Error, Errors = [new() { Message = "Payment authorization failed." }] };
            }
        }
    }
}
