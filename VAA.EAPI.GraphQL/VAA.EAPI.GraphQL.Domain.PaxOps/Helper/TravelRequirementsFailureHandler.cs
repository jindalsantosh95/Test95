using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class TravelRequirementsFailureHandler
    {
        private static readonly ISet<string> ClientSafeErrorCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "travelrequirement_020_003"
        };

        public static async Task<TravelRequirementsResponse> HandleAsync(
            IResolverContext context,
            Exception exception,
            TravelRequirementsFailureContext failureContext,
            ITelemetryService telemetryService,
            ILogger logger)
        {
            var pnrLocator = failureContext.PnrLocator;
            if (pnrLocator.IsNullOrEmpty() && !failureContext.TravelRequirementsId.IsNullOrEmpty())
            {
                try
                {
                    pnrLocator = await TravelRequirementsIdHelper.GetPnrLocator(failureContext.TravelRequirementsId);
                }
                catch (Exception pnrException)
                {
                    logger.LogWarning(
                        pnrException,
                        "Unable to resolve the PNR for failed travel requirements operation {Operation}",
                        failureContext.Operation);
                }
            }

            logger.LogError(
                exception,
                "Travel requirements operation {Operation} failed for PNR {PnrLocator} with HTTP status {HttpStatusCode}",
                failureContext.Operation,
                pnrLocator,
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
                        "Travel requirements error response JSON deserialization failed for {Operation}",
                        failureContext.Operation);
                }
            }

            var returnParsedError = error is not null &&
                (failureContext.ReturnParsedError || ClientSafeErrorCodes.Contains(error.Code));

            var telemetryError = error is null || returnParsedError
                ? error
                : new ErrorDetails
                {
                    Code = error.Code,
                    Message = error.Message
                };

            _ = telemetryService.TrackTelemetry((TelemetryEvent)(
                LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED,
                Data: TelemetryHelper.TravelRequirementsFailed(
                    pnrLocator,
                    failureContext.LoggingFailureContext,
                    telemetryError)));

            return returnParsedError
                ? ErrorHelper.Fail<TravelRequirementsResponse>(context, error.Message, error.Title, error.Code)
                : ErrorHelper.Fail<TravelRequirementsResponse>(
                    context,
                    failureContext.DefaultMessage,
                    failureContext.DefaultTitle,
                    failureContext.DefaultCode);
        }
    }

}
