using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json.Linq;
using System.Net;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;
using VAA.EAPI.GraphQL.Stitch.Core.Serialisation;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
{
    public sealed class RequestValidationMiddleware : IFunctionsWorkerMiddleware
    {
        private const string InjectionPattern = @"<[^>]+>|(on\w+\s*=)|javascript:";
        private const string InlineInjectionPattern = @"""[^""]+""";

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
            var query = (context.BindingContext.BindingData.TryGetValue("query", out object value) ? value : null)?.ToString() ?? string.Empty;
            var mutation = (context.BindingContext.BindingData.TryGetValue("mutation", out value) ? value : null)?.ToString() ?? string.Empty;
            var variables = Json.Deserialize<JObject>((context.BindingContext.BindingData.TryGetValue("variables", out value) ? value : null)?.ToString() ?? "{}");

            if (ContainsInjection(variables))
            {
                context.SetHttpResponseData(HttpStatusCode.OK, Json.Serialize(new { Errors = new[] { new { Message = "Possible injection detected in variables.", Extensions = new { Code = "400" } } } }));

                return;
            }

            var inlineQueryValues = RegexCompiled.Matches(query, InlineInjectionPattern)
                                            .AsParallel()
                                            .Where(x => RegexCompiled.IsMatch(x.Value, InjectionPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                            .ToArray();

            if (inlineQueryValues.Length != 0)
            {
                context.SetHttpResponseData(HttpStatusCode.OK, Json.Serialize(new { Errors = new[] { new { Message = $"Possible injection detected in query.{Environment.NewLine}{string.Join(Environment.NewLine, inlineQueryValues.Select(x => x.Value))}", Extensions = new { Code = "400" } } } }));

                return;
            }

            var inlineMutationValues = RegexCompiled.Matches(query, InlineInjectionPattern)
                                            .AsParallel()
                                            .Where(x => RegexCompiled.IsMatch(x.Value, InjectionPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                            .ToArray();

            if (inlineMutationValues.Length != 0)
            {
                context.SetHttpResponseData(HttpStatusCode.OK, Json.Serialize(new { Errors = new[] { new { Message = $"Possible injection detected in mutation.{Environment.NewLine}{string.Join(Environment.NewLine, inlineMutationValues.Select(x => x.Value))}", Extensions = new { Code = "400" } } } }));

                return;
            }

            await next(context);
        }
    }
}
