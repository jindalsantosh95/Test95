using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Services.CjsCheckout
{
    public sealed class CjsCheckoutPaymentApp : ICjsCheckoutPaymentApp
    {
        private readonly ICjsCheckoutPaymentAppGateway _gateway;
        private readonly IMmbQueryLoginGateway _mmbQueryLoginGateway;
        private readonly ILogger<CjsCheckoutPaymentApp> _logger;

        public CjsCheckoutPaymentApp(
            ICjsCheckoutPaymentAppGateway gateway,
            IMmbQueryLoginGateway mmbQueryLoginGateway,
            ILogger<CjsCheckoutPaymentApp> logger)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _mmbQueryLoginGateway = mmbQueryLoginGateway ?? throw new ArgumentNullException(nameof(mmbQueryLoginGateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CjsCheckoutPaymentAppResponse>>> HandleAsync(
            string requestBody,
            CancellationToken cancellationToken)
        {
            var correlationId = (AsyncContext.Current?.CorrelationId ?? Guid.NewGuid()).ToString();

            if (string.IsNullOrWhiteSpace(requestBody))
                return Result.Success(BadRequest(correlationId));

            CjsCheckoutPaymentAppRequest parsedBody;
            try
            {
                parsedBody = Json.Deserialize<CjsCheckoutPaymentAppRequest>(requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid JSON payload received for CJS payment-app call. CorrelationId: {CorrelationId}", correlationId);
                return Result.Success(BadRequest(correlationId));
            }

            if (parsedBody is null
                || string.IsNullOrWhiteSpace(parsedBody.BookingReference)
                || string.IsNullOrWhiteSpace(parsedBody.Surname)
                || string.IsNullOrWhiteSpace(parsedBody.DateOfBirth))
                return Result.Success(BadRequest(correlationId));

            try
            {
                var login = await _mmbQueryLoginGateway.LoginAsync(
                    new MmbQueryLoginRequest(parsedBody.BookingReference, parsedBody.Surname, parsedBody.DateOfBirth),
                    cancellationToken);

                if (login.StatusCode is < HttpStatusCode.OK or >= HttpStatusCode.MultipleChoices || login.Body is null)
                {
                    var errorsLogin = ErrorMapper.MapTechnicalExceptionToEAPIException(
                        ErrorCodes.HolidayPaymentLoginError,
                        correlationId,
                        EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                    return Result.Success(new Response<CjsCheckoutPaymentAppResponse>
                    {
                        Status = MapStatus(login.StatusCode),
                        Errors = errorsLogin?.ToList()
                    });
                }
                var cookieHeader = login.CookieHeader;

                var payload = new CjsCheckoutPaymentAppPayload(
                    parsedBody.ReturnUrl,
                    parsedBody.PayPalClientUrl,
                    parsedBody.ExpiryUrl,
                    parsedBody.ContextPath);

                var downstream = await _gateway.StartPaymentSessionAsync(payload, cookieHeader: cookieHeader, cancellationToken);

                if (downstream.StatusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices
                    && parsedBody.AmountToPay is not null)
                {
                    //validate that downstream.Payment session amount to pay is greater than zero and greater than the amount in the request body
                    if ((parsedBody.AmountToPay <= 0) || (downstream.Body.PaymentSessions?.FirstOrDefault()?.AmountToPay < parsedBody.AmountToPay))
                    {
                        var errorsValidation = ErrorMapper.MapTechnicalExceptionToEAPIException(
                            ErrorCodes.HolidayPaymentValidationError,
                            correlationId,
                            EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                        return Result.Success(new Response<CjsCheckoutPaymentAppResponse>
                        {
                            Status = Status.BadRequest,
                            Errors = errorsValidation?.ToList()
                        });
                    }
                    
                    var cookies = (downstream.SetCookieHeaders ?? cookieHeader ?? string.Empty)
                        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(p => p.Contains('='))
                        .ToArray();

                    downstream = await _gateway.SetPaymentAmountAsync(parsedBody.AmountToPay.Value, cookies, cancellationToken);
                }

                if (downstream.StatusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices)
                {
                    if (downstream.Body is null)
                        throw new InvalidOperationException("CJS checkout payment-app returned success status but response body was empty or invalid.");

                    // downstream.SetCookieHeaders is a Cookie header-style string containing cookie pairs separated by ';'
                    // ResultExtention binds headers by calling response.Headers.Add(key, value) per entry, so emit one Set-Cookie per pair.
                    var headerParameters = string.IsNullOrWhiteSpace(downstream.SetCookieHeaders)
                        ? Array.Empty<ResponseHeader>()
                        : downstream.SetCookieHeaders
                            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Where(p => p.Contains('='))
                             .Select(p => new ResponseHeader { Key = "Set-Cookie", Value = p }).ToArray();

                    return Result.Success(new Response<CjsCheckoutPaymentAppResponse>
                    {
                        Status = Status.OK,
                        HeaderParameters = headerParameters.ToList(),
                        Value = downstream.Body
                    });
                }

                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(
                    ErrorCodes.HolidayPaymentProcessingError,
                    correlationId,
                    EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                return Result.Success(new Response<CjsCheckoutPaymentAppResponse>
                {
                    Status = MapStatus(downstream.StatusCode),
                    Errors = errors?.ToList()
                });
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "CJS payment-app authentication failed. CorrelationId: {CorrelationId}", correlationId);

                return Result.Failure<Response<CjsCheckoutPaymentAppResponse>>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CJS payment-app call failed. CorrelationId: {CorrelationId}", correlationId);

                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(
                    ErrorCodes.HolidayPaymentProcessingError,
                    correlationId,
                    EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

                return Result.Failure<Response<CjsCheckoutPaymentAppResponse>>(Json.Serialize(errors));
            }
        }

        private static Response<CjsCheckoutPaymentAppResponse> BadRequest(string correlationId)
        {
            var errors = ErrorMapper.MapBusinessExceptionToEAPIException(
                ErrorCodes.HolidayPaymentValidationError,
                correlationId,
                EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);

            return new Response<CjsCheckoutPaymentAppResponse>
            {
                Status = Status.BadRequest,
                Errors = errors?.ToList()
            };
        }

        private static Status MapStatus(HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.BadRequest => Status.BadRequest,
                HttpStatusCode.Unauthorized => Status.Unauthorized,
                HttpStatusCode.Forbidden => Status.Forbidden,
                HttpStatusCode.NotFound => Status.NotFound,
                _ => Status.Error
            };
    }
}
