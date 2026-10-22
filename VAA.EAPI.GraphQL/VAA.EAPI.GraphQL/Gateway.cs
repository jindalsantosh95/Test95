using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL
{
    public sealed class Gateway
    {
        private readonly IHttpRequestParser _requestParser;
        private readonly IHttpResultSerializer _resultSerializer;
        private readonly IRequestExecutorResolver _requestExecutorResolver;
        private readonly ILogger<Gateway> _logger;

        public Gateway(IHttpRequestParser requestParser, IHttpResultSerializer resultSerializer, IRequestExecutorResolver requestExecutorResolver, ILogger<Gateway> logger)
        {
            _requestParser = requestParser ?? throw new ArgumentNullException(nameof(requestParser));
            _resultSerializer = resultSerializer ?? throw new ArgumentNullException(nameof(resultSerializer));
            _requestExecutorResolver = requestExecutorResolver ?? throw new ArgumentNullException(nameof(requestExecutorResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal async Task<HttpResponseData> ExecuteQueryAsync(HttpRequestData req)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Check to see if the request is for a persisted query
            Stream reqBody = CheckForPersistedQuery(req);
            string queryName = null;

            IExecutionResult result = default;
            try
            {
                // Store the HTTP Request Headers in the cargo-hold for potential use by queries / mutations.
                AsyncContext.Current.Context.Items.Add("RequestHeaders", req.Headers);

                var requests = await _requestParser.ReadJsonRequestAsync(reqBody, CancellationToken.None).ConfigureAwait(false);
                var executor = await _requestExecutorResolver.GetRequestExecutorAsync().ConfigureAwait(false);
                var batchRequest = requests.Select(r => QueryRequestBuilder.From(r).Create());

                var st = requests.First().Query.ToString();
                try
                {
                    // Assumed to be a query call, starting { and 5 whitespace characters. ) marks the end of the query name
                    // If there are multiple queries sent in one request, only the first will be logged.
                    if (st.Contains('(')) queryName = st.Substring(0, st.IndexOf('(')).Trim();
                }
                catch (Exception) { }
                queryName ??= st.Length > 50 ? string.Concat(st.AsSpan(0, 50), "...") : st;

                result = await executor.ExecuteBatchAsync(batchRequest).ConfigureAwait(false);
            }
            catch (GraphQLRequestException ex)
            {
                result = QueryResultBuilder.CreateError(ex.Errors);
                queryName ??= "Exception: " + ex.Message;
            }
            catch (Exception ex)
            {
                result = QueryResultBuilder.CreateError(new Error(ex.Message));
                queryName ??= ex.GetType().ToString() + ": " + ex.Message;
            }

            var response = await PrepareResponse(req, result).ConfigureAwait(false);

            _logger.LogInformation($"GraphQL:{queryName}, Duration={stopwatch.Elapsed.TotalMilliseconds}ms");
            System.Diagnostics.Debug.WriteLine($"GraphQL:{queryName}, Duration={stopwatch.Elapsed.TotalMilliseconds}ms");

            return response;
        }

        /// <summary>
        /// Checks to see if there is an "id" field in the request body or query string.
        /// 
        /// If one exists, it will load the query from the relevant store file, merge in any other post data (e.g. variables) and 
        /// then recreate the request body (as a memory stream)
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private Stream CheckForPersistedQuery(HttpRequestData req)
        {
            GraphQLModel jsonBody = new();

            // Load BODY into object
            if (req.Body != Stream.Null)
            {
                var reqBody = System.Text.Encoding.UTF8.GetString((req.Body as MemoryStream).ToArray());
                jsonBody = Json.Deserialize<GraphQLModel>(reqBody);
            }

            if ((!jsonBody.Query?.Contains("__schema", StringComparison.OrdinalIgnoreCase)) ?? true) // Ignore any intrspection request
            {
                // Find a Query Id
                string queryId = string.Empty;

                // Check for Query Id in the query string
                var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                if (queryParams["pqId"] != null)
                {
                    queryId = queryParams["pqId"].ToString();
                }
                // Check for Query Id in body
                else if (!string.IsNullOrEmpty(jsonBody.PqId))
                {
                    queryId = jsonBody.PqId;
                }

                // Check for variables query string
                if (queryParams["variables"] != null)
                {
                    jsonBody.Variables = JObject.Parse(queryParams["variables"].ToString());
                }

                // Read in the Persisted file content
                if (!string.IsNullOrWhiteSpace(queryId))
                {
                    string queryFile = string.Format("{0}\\{1}\\{2}.graphql",
                        Common.Constants.Application.FunctionAppDirPath,
                        ConfigReader.Get(Common.Constants.Environment.PersistedQueryStore),
                        queryId);

                    if (File.Exists(queryFile))
                    {
                        jsonBody.Query = System.IO.File.ReadAllText(queryFile);
                    }
                    else
                    {
                        throw new PersistedQueryException($"Persisted query with Id {queryId} does not exist");
                    }
                }
            }

            return (new MemoryStream(Encoding.UTF8.GetBytes(Json.Serialize(jsonBody))));
        }

        private async Task<HttpResponseData> PrepareResponse(HttpRequestData requestData, IExecutionResult queryResult)
        {
            var response = requestData.CreateResponse();
            //response.Headers.Add("Content-Type", _resultSerializer.GetContentType(queryResult));
            response.Headers.Add("Content-Type", "application/json");
            response.StatusCode = _resultSerializer.GetStatusCode(queryResult);
            var json = await SerializeResult(queryResult).ConfigureAwait(false);

            // Add any headers created by the query / mutation
            if (AsyncContext.Current.Context.Items.ContainsKey(Common.Constants.Application.ResponseHeaderKey))
            {
                var headerObject = AsyncContext.Current.Context.Items[Common.Constants.Application.ResponseHeaderKey];

                if (headerObject is IDictionary<string, string?> headerDictionary)
                {
                    foreach (var (key, value) in headerDictionary)
                        AddResponseHeaderValues(response, key, value);
                }                
                else if (headerObject is IEnumerable<KeyValuePair<string, string?>> headerPairs)
                {
                    foreach (var pair in headerPairs)
                        AddResponseHeaderValues(response, pair.Key, pair.Value);
                }               
            }

            await response.WriteStringAsync(json).ConfigureAwait(false);
            return response;
        }

        private static void AddResponseHeaderValues(HttpResponseData response, string key, string? value)
        {
            if (response is null)
                return;

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return;

            var parts = value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
                return;

            response.Headers.Add(key, parts);
        }

        private static async Task<string> SerializeResult(IExecutionResult queryResult)
        {
            if (queryResult is IBatchQueryResult batchQueryResult)
            {
                var sb = new StringBuilder();
                await foreach (IQueryResult payload in batchQueryResult.ReadResultsAsync())
                {
                    var json = await payload.ToJsonAsync(false).ConfigureAwait(false);
                    sb.AppendLine(json);
                }

                return sb.ToString();
            }

            return await queryResult.ToJsonAsync(false).ConfigureAwait(false);
        }
    }


    public class GraphQLModel
    {
        public string PqId { get; set; }
        public string Query { get; set; }
        public JObject Variables { get; set; }
    }
}
