using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class SecureFlightInfoFailureHandler
    {
        public static async Task<T> HandleAsync<T>(
            IResolverContext context,
            Exception exception,
            SecureFlightInfoFailureContext failureContext,
            ITelemetryService telemetryService,
            ILogger logger)
        {
            logger.LogError(
                "Secure flight information operation {Operation} failed with HTTP status {HttpStatusCode}",
                failureContext.Operation,
                (exception as HttpRequestException)?.StatusCode);

            ErrorDetails? error = null;
            var responseJson = exception.Data?["Response"]?.ToString();

            if (!responseJson.IsNullOrEmpty())
            {
                try
                {
                    error = Json.Deserialize<List<ErrorDetails>>(responseJson)?.FirstOrDefault();
                }
                catch (Exception parseException)
                {
                    logger.LogWarning(
                        parseException,
                        "Secure flight information error response JSON deserialization failed for {Operation}",
                        failureContext.Operation);
                }
            }

            var telemetryError = error is null || failureContext.ReturnParsedError
                ? error
                : new ErrorDetails
                {
                    Code = error.Code,
                    Message = error.Message,
                    Title = error.Title
                };

            _ = telemetryService.TrackTelemetry((TelemetryEvent)(
                LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_FAILED,
                Data: TelemetryHelper.SecureFlightInfoFailed(
                    failureContext.PnrLocator,
                    failureContext.LoggingFailureContext,
                    telemetryError,
                    failureContext.SecureFlightReservationId)));

            return error is not null && failureContext.ReturnParsedError
                ? ErrorHelper.Fail<T>(context, error.Message, error.Title, error.Code)
                : ErrorHelper.Fail<T>(
                    context,
                    failureContext.DefaultMessage,
                    failureContext.DefaultTitle,
                    error?.Code ?? failureContext.DefaultCode);
        }
    }
}
