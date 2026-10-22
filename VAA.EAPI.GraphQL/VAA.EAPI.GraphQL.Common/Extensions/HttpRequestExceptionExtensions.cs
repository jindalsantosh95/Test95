using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpRequestExceptionExtensions
    {
        public static async Task<string> ReadResponseContentAsStringAsync(this HttpRequestException source) => await source.ReadResponseContentAsStringAsync(CancellationToken.None);
        public static async Task<string> ReadResponseContentAsStringAsync(this HttpRequestException source, CancellationToken cancellationToken)
        {
            if (source?.Data["Sensitive"] is true)
                return "[REDACTED]";

            return source?.InnerException is HttpResponseException ex
                ? await ex.ReadResponseContentAsStringAsync(cancellationToken)
                : string.Empty;
        }
    }
}
