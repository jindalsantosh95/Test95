using Microsoft.Azure.Functions.Worker.Http;

namespace VAA.EAPI.Web.Common.Infrastructure.Security
{
    public class SecurityHeaders
    {
        public static void BindResponseSecurityHeaders(HttpResponseData httpResponseData)
        {
            foreach (var header in Constants.Application.SecurityHeaders)
                httpResponseData.Headers.Add(header.Key, header.Value);
        }
    }
}