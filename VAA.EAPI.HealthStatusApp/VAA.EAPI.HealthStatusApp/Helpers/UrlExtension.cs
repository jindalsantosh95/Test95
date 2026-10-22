using Microsoft.Graph;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public static class UrlHelper
    {
        public static string GetSubdomain(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            try
            {
                var uri = new Uri(url);
                return uri.Host.Split('.').FirstOrDefault() ?? string.Empty;
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Invalid URL format", nameof(url), ex);
            }
        }

        public static string GetGraphQLCoreName(this string url)
        {
            if (url.EndsWith("search/public/heartbeat", StringComparison.OrdinalIgnoreCase))
                return "search";

            var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var index = Array.FindIndex(parts, part => part.Equals("GraphQL", StringComparison.OrdinalIgnoreCase)) + 1;
            return parts[index].ToLower() switch
            {
                "public" => "main",
                "secure" => "main",
                "graphql" => url,
                _ => parts[index]
            };
        }

        public static string GetSourceName(this string url)
        {
            var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var heartbeatIndex = Array.FindIndex(parts, part => part.Equals("heartbeat", StringComparison.OrdinalIgnoreCase));

            if (heartbeatIndex <= 0)
            {
                throw new ArgumentException("Invalid URL: 'heartbeat' not found", nameof(url));
            }

            //This is needed as Search GraphQl does not have graph in base url
            if (url.Contains("search/public", StringComparison.OrdinalIgnoreCase))
            {
                return "Search : GraphQL Public";
            }
            else if (url.Contains("/search/", StringComparison.OrdinalIgnoreCase))
            {
                return "Search : GraphQL";
            }

            if (url.Contains("graphql", StringComparison.OrdinalIgnoreCase))
            {
                if(parts[heartbeatIndex - 1] == "public")
                {
                    return $"{parts[heartbeatIndex - 2]} : {parts[heartbeatIndex - 3]} {parts[heartbeatIndex - 1]}";
                }
                else
                {
                    return $"{parts[heartbeatIndex - 1]} : {parts[heartbeatIndex - 2]}";
                }
            }

            return parts[heartbeatIndex - 1];
        }
    }
}