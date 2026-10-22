using Microsoft.Azure.Functions.Worker.Http;

namespace VAA.EAPI.Infrastructure.Security
{
    public class SecurityHeaders
    {
        public static void BindResponseSecurityHeaders(HttpResponseData httpResponseData)
        {
            foreach (var header in Constants.Constants.SecurityHeaders)
                httpResponseData.Headers.Add(header.Key, header.Value);
        }
    }
}