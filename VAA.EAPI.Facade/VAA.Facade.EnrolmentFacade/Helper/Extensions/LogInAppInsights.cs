using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace VAA.Facade.EnrolmentFacade.Helper.Extensions
{
    public static class LogInAppInsights
    {
        public static void LogClientIDInApplicationInsights(this string httpRequest)
        {
            string jwt = httpRequest.Replace("Bearer ", string.Empty);
            JwtSecurityToken token = new JwtSecurityToken(jwt);
            string Clientid = token.Claims.First(claim => claim.Type == "appid").Value;

            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = ConfigReader.GetConfigurationSettings(Constants.Constants.ApplicationInsightKey);
            TelemetryClient telemetryClient = new TelemetryClient(configuration);
            Dictionary<string, string> customProperties = new Dictionary<string, string>
            {
                {"ClientId",Clientid}
            };
            telemetryClient.TrackTrace("ClientID", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information, customProperties);
        }

    }
}
