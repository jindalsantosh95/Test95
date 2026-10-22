using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VAA.EAPI.GraphQL.Stitch.Core.Exceptions;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
{
    public sealed class PeristedQueryMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequestData = await context.GetHttpRequestDataAsync();

            if (httpRequestData.Method.Is("post"))
            {
                var isPersistedRequest = context.BindingContext.BindingData.Any(x => x.Key.IsAny("pqid"));

                if (isPersistedRequest)
                {
                    var id = context.BindingContext.BindingData.First(x => x.Key.IsAny("pqid")).Value.ToString();
                    var filePath = $"{Constants.Application.FunctionAppDirPath}\\{ConfigReader.Get(Constants.Environment.PersistedQueryStore)}\\{id}.graphql";

                    if (!File.Exists(filePath)) throw new PersistedQueryException($"Persisted query with Id {id} does not exist.");

                    var dictionary = context.BindingContext.BindingData.GetType()
                                                                       .GetField("m_dictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                                                       .GetValue(context.BindingContext.BindingData) as Dictionary<string, object>;

                    var query = File.ReadAllText(filePath);

                    dictionary["query"] = query;

                    dictionary.Remove("pqid");
                    dictionary.Add("operationName", query.Substring(query.IndexOf(' ') + 1, query.IndexOf('(') - query.IndexOf(' ') - 1));

                    var requestObject = JsonConvert.DeserializeObject<JObject>("{}");

                    requestObject.Add("query", dictionary["query"].ToString());
                    requestObject.Add("variables", JsonConvert.DeserializeObject<JObject>(dictionary["variables"].ToString()));
                    requestObject.Add("operationName", dictionary["operationName"].ToString());

                    var buffer = Encoding.UTF8.GetBytes(requestObject.ToString());
                    var body = httpRequestData.Body;

                    body.Position = 0;

                    body.Write(buffer, 0, buffer.Length);

                    body.Position = 0;

                    await next(context);

                    return;
                }
            }

            await next(context);
        }
    }
}
