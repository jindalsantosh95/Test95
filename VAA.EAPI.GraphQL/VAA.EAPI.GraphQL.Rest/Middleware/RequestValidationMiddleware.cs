using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Rest.Middleware
{
    public sealed class RequestValidationMiddleware : IFunctionsWorkerMiddleware
    {
        private const string InjectionPattern = @"<[^>]+>|(on\w+\s*=)|javascript:";

        private static bool ContainsInjection(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:

                    foreach (var property in ((JObject)token).Properties())
                        if (ContainsInjection(property.Value))
                            return true;

                    break;
                case JTokenType.Array:

                    foreach (var item in (JArray)token)
                        if (ContainsInjection(item))
                            return true;

                    break;
                case JTokenType.String:

                    var value = token.ToString();

                    if (RegexCompiled.IsMatch(value, InjectionPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        return true;

                    break;
            }

            return false;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequest = await context.GetHttpRequestDataAsync();

            if (httpRequest == null)
            {
                await next(context);

                return;
            }

            var body = await httpRequest.ReadAsStringAsync();

            if (httpRequest.Body.CanSeek)
                httpRequest.Body.Position = 0;

            if (body.IsNullOrWhiteSpace())
                body = "{}";

            JObject request;

            try
            {
                request = Json.Deserialize<JObject>(body);
            }
            catch
            {
                context.SetHttpResponseData(HttpStatusCode.BadRequest, Json.Serialize(new { Errors = new[] { new { Message = "Malformed JSON in request body.", Extensions = new { Code = "400" } } } }));

                return;
            }

            if (ContainsInjection(request))
            {
                context.SetHttpResponseData(HttpStatusCode.BadRequest, Json.Serialize(new { Errors = new[] { new { Message = "Possible injection detected in request.", Extensions = new { Code = "400" } } } }));

                return;
            }

            await next(context);
        }
    }
}
