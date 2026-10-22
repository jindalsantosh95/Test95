using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class BaggageFailureHandler
    {
        public static async Task<TResponse> HandleAsync<TResponse>(
            IResolverContext context,
            Exception exception,
            BaggageFailureContext failureContext,
            ITelemetryService telemetryService,
            ILogger logger)
        {
            string? pnrLocator = null;

            if (!failureContext.CheckInId.IsNullOrWhiteSpace())
            {
                try
                {
                    pnrLocator = await CheckInIdHelper.GetPnrLocator(failureContext.CheckInId);
                }
                catch (Exception pnrException)
                {
                    logger.LogWarning(
                        pnrException,
                        "Unable to resolve the PNR for failed baggage operation {Operation}",
                        failureContext.Operation);
                }
            }

            logger.LogError(
                exception,
                "Baggage operation {Operation} failed for PNR {PnrLocator} with HTTP status {HttpStatusCode}",
                failureContext.Operation,
                pnrLocator,
                exception is HttpRequestException { StatusCode: { } statusCode } ? (int?)statusCode : null);

            _ = telemetryService.TrackTelemetry((TelemetryEvent)(
                LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED,
                Data: await TelemetryHelper.BaggageDetailsFailed(
                    failureContext.CheckInId,
                    failureContext.IsPaidBaggage,
                    failureContext.LoggingFailureContext)));

            return ErrorHelper.Fail<TResponse>(
                context,
                failureContext.DefaultMessage,
                failureContext.DefaultTitle,
                failureContext.DefaultCode);
        }
    }

}
