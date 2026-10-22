using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Common.Diagnostics.Logging
{
    public static class LoggingProviderResolver
    {
        public static LoggingProvider FromEnvironment() =>
            Parse(ConfigReader.Try(Constants.Environment.LoggingProvider, nameof(LoggingProvider.AppInsights)));

        public static LoggingProvider Parse(string configuredValue)
        {
            foreach (var supportedProvider in System.Enum.GetValues<LoggingProvider>())
            {
                if (supportedProvider.ToString().Equals(configuredValue, System.StringComparison.OrdinalIgnoreCase))
                    return supportedProvider;
            }

            return LoggingProvider.AppInsights;
        }
    }
}
