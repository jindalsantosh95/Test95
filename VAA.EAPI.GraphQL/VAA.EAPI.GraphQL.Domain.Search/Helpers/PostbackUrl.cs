using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public static class PostbackUrl
    {
        public static string Get()
        {
            if (AsyncContext.Current.Context.Items.TryGetValue("RequestHeaders", out var headers))
            {
                if (((HttpHeadersCollection)headers).TryGetValues("postbackurl", out var postbackurl))
                {
                    return postbackurl.First();
                }
            }
            return string.Empty;
        }
    }
}
