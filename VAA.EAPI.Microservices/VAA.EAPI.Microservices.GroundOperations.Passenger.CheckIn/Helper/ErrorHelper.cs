using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper
{
    public class ErrorHelper
    {
        public static ErrorDetails TravelRequirementsErrorMapper(HttpRequestException source, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            string? errorCode = null;

            var responseJson = source?.Data["Response"]?.ToString();

            if (!string.IsNullOrWhiteSpace(responseJson))
            {
                try
                {
                    var payload = JObject.Parse(responseJson);
                    var rootCode = GetFirstExceptionCode(payload["exceptions"]);
                    var batchCode = payload["batchResults"] is JArray batchResults
                        ? GetFirstExceptionCode(batchResults.FirstOrDefault()?["exceptions"])
                        : null;

                    errorCode = new[] { rootCode, batchCode, payload["code"]?.ToString() }
                        .FirstOrDefault(code => !code.IsNullOrWhiteSpace());
                }
                catch
                {
                    logger.LogWarning("Travel requirements error response JSON deserialization failed.");
                }
            }

            if (errorCode.IsNullOrWhiteSpace() && source.StatusCode is { } statusCode)
            {
                errorCode = $"HTTP_{(int)statusCode}";
            }

            var trm = CheckInMessageHelper.GetTrmErrorMessageTravelRequirements(errorCode);

            return new ErrorDetails
            {
                Code = errorCode,
                Title = trm.Item1,
                Message = trm.Item2
            };
        }

        private static string? GetFirstExceptionCode(JToken? exceptions) =>
            exceptions is JArray exceptionItems
                ? exceptionItems.FirstOrDefault()?["code"]?.ToString()
                : null;
    }
}
