using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class OnlineCheckInStatus
    {
        public bool Available { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public string DeveloperMessage { get; set; }
        public string? AdvisoryText1 { get; set; }

        static string ReplaceTokens(string template, Dictionary<string, string> replacements) =>
            RegexCompiled.Replace(template, @"{(\w+)}",
                match => replacements.TryGetValue(match.Groups[1].Value, out string replacement) ? replacement : match.Value);

        // This is for mapping HTTP 4xx class errors from TRM. 5xx errors from any API can also be handled with the default error message.
        public static explicit operator OnlineCheckInStatus(HttpRequestException source)
        {
            var responseJson = source?.Data["Response"]?.ToString();

            if (!responseJson.IsNullOrEmpty())
            {
                var apiError = Json.Deserialize<APIErrorResponse>(responseJson);
                var exception = apiError?.BatchResults?.FirstOrDefault()?.Exceptions?.FirstOrDefault();
                var trmErrorMessage = CheckInMessageHelper.GetTrmErrorMessage(exception?.Code ?? apiError?.Code);

                return new OnlineCheckInStatus()
                {
                    Available = false,
                    Code = exception?.Code ?? apiError?.Code,
                    Title = trmErrorMessage?.Item1,
                    Message = trmErrorMessage?.Item2
                };

            }

            var exceptionCode = source.StatusCode.IsNullOrDefault() ? source.Message + source.InnerException?.Message + Json.Serialize(source.Data) : $"HTTP_{(int?)source.StatusCode}";
            var statusMessage = CheckInMessageHelper.GetStatusMessage(exceptionCode);

            return new OnlineCheckInStatus()
            {
                Available = false,
                Title = statusMessage.Item1,
                Message = statusMessage.Item2,
                Code = exceptionCode
            };
        }

        public static explicit operator OnlineCheckInStatus(APIErrorResponse source)
        {
            var exception = source?.BatchResults?.FirstOrDefault()?.Exceptions?.FirstOrDefault();

            return new OnlineCheckInStatus()
            {
                Available = false,
                Code = exception?.Code ?? source?.Code,
                Title = exception?.Title ?? source?.Title,
                Message = exception?.Message ?? source?.Message
            };
        }

        public static explicit operator OnlineCheckInStatus(DeltaSsci.TravelParty source)
        {
            if (source.CheckinAdvisory != null)
            {
                var statusMessage = CheckInMessageHelper.GetStatusMessage(source.CheckinAdvisory.ErrorMessage);

                Dictionary<string, string>? replacements;
                switch (source.CheckinAdvisory.ErrorMessage)
                {
                    case "redirectJVPartner":
                        replacements = new Dictionary<string, string>
                        {
                            ["firstAirlineName"] = source.CheckinAdvisory.AdvisoryText3
                        };
                        break;

                    default:
                        replacements = null;
                        break;
                }

                return new OnlineCheckInStatus()
                {
                    Available = false,
                    Title = replacements == null ? statusMessage.Item1 : ReplaceTokens(statusMessage.Item1, replacements),
                    Message = replacements == null ? statusMessage.Item2 : ReplaceTokens(statusMessage.Item2, replacements),
                    Code = source.CheckinAdvisory.ErrorMessage,
                    AdvisoryText1 = System.Net.WebUtility.HtmlDecode(source.CheckinAdvisory?.AdvisoryText1)
                };
            }
            else if (source.FirstSegmentOa == true)
            {
                Dictionary<string, string>? replacements;
                var statusMessage = CheckInMessageHelper.GetStatusMessage(Constants.Application.RedirectJVPartner);
                replacements = new Dictionary<string, string>
                {
                    ["firstAirlineName"] = source?.Trip?.OperatedByAirlineName
                };
                return new OnlineCheckInStatus()
                {
                    Available = false,
                    Title = replacements == null ? statusMessage.Item1 : ReplaceTokens(statusMessage.Item1, replacements),
                    Message = replacements == null ? statusMessage.Item2 : ReplaceTokens(statusMessage.Item2, replacements),
                    Code = Constants.Application.RedirectJVPartner
                };

            }
            else
            {

                return new OnlineCheckInStatus() { Available = true };
            }
        }
    }
}
