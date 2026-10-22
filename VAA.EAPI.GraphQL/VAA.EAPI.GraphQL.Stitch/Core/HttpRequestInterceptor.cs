using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace VAA.EAPI.GraphQL.Stitch.Core
{

    public sealed class HttpRequestInterceptor : DefaultHttpRequestInterceptor
    {
        public override async ValueTask OnCreateAsync(HttpContext context, IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);

            if (context.Request.Headers.ContainsKey("x-allow-introspection"))
                requestBuilder.AllowIntrospection();
        }
    }
}
