using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public static class ConfigReader
    {
        private static readonly string[] ExcludedHeaders = ["postman-token"];
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        private static AsyncLocal<ConcurrentDictionary<string, object>> _cache;
        private static ConcurrentDictionary<string, object> Cache
        {
            get
            {
                if (_cache is null || _cache.Value is null)
                {
                    semaphoreSlim.Wait();

                    if (_cache is null || _cache.Value is null)
                    {
                        _cache ??= new() { Value = new(StringComparer.OrdinalIgnoreCase) };
                        _cache.Value ??= new(StringComparer.OrdinalIgnoreCase);
                    }

                    semaphoreSlim.Release();
                }

                return _cache.Value;
            }
        }

        public static string ReadAllText(string rootPath, string filePath) => ReadAllText(Path.GetFullPath(Path.Combine(rootPath, filePath)));
        public static string ReadAllText(string filePath) => File.ReadAllText(filePath);

        public static string Get(string key) => Get<string>(key);

        public static T Get<T>(string key)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(key);

            if (Cache.TryGetValue(key, out object value))
            {
                if (value is T castedValue) return castedValue;

                if (value is string stringValue && stringValue.TryParse(out T parsedValue)) return parsedValue;
            }

            string configValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            if (configValue.IsNullOrEmpty())
                throw new NullReferenceException(key);

            if (!configValue.TryParse(out T result))
                throw new NotSupportedException($"{typeof(T)} is not supported.");

            Cache.AddOrUpdate(key, result, (k, v) => result);

            return result;
        }

        public static void Set(string key, string value)
        {
            if (key.IsNullOrWhiteSpace()) throw new ArgumentNullException(key);

            Cache[key] = value;

            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        }

        public static string Try(string key, string @default = default) => Try<string>(key, @default);

        public static T Try<T>(string key, T @default = default)
        {
            if (key.IsNullOrWhiteSpace()) return @default;

            if (Cache.TryGetValue(key, out object value))
            {
                if (value is T castedValue) return castedValue;

                if (value is string stringValue && stringValue.TryParse(out T parsedValue)) return parsedValue;
            }

            string configValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

            if (configValue.IsNullOrWhiteSpace())
                return @default;

            if (configValue.TryParse(out T result, @default))
                Cache.AddOrUpdate(key, result, (k, v) => result);

            return result ?? @default;
        }

        public static string Find(string key, string filter = "") => Find(key, filter, string.Empty);
        public static T Find<T>(string key, string filter = "", T @default = default)
        {
            var variables = Find(filter);

            foreach (var variable in variables)
                if (RegexCompiled.IsMatch(key, HttpUtility.UrlDecode(variable), RegexOptions.Singleline | RegexOptions.IgnoreCase))
                    return Try(variable, @default);

            return @default;
        }
        public static string[] Find(string filter)
        {
            var variables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Keys.OfType<string>().ToArray();

            if (filter.IsNullOrEmpty()) return [.. variables];

            return [.. variables.Where(f => RegexCompiled.IsMatch(f, filter, RegexOptions.Singleline | RegexOptions.IgnoreCase))];
        }

        public static class Caching
        {
            private const string WhitespaceRegexPattern = "\\s+";
            private static readonly RegexOptions WhitespaceRegexOptions = RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;

            public static bool IsCacheEnabled(string operationName, out string key, out TimeSpan expiry, out bool isSliding, params string[] tokens)
            {
                var value = Find($"Cache:{operationName.ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;
                expiry = TimeSpan.Zero;
                isSliding = true;

                if (value.IsNullOrWhiteSpace()) return false;

                var config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = !config["isSliding"].TryParse<bool>(out var result) || result;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                key = isCachedEnabled ? BuildKey(operationName, tokens) : string.Empty;

                return isCachedEnabled;
            }
            public static bool IsCacheEnabled(FunctionContext functionContext, out string key)
            {
                var operationName = functionContext.GetOperationName();
                var value = Find($"Cache:{operationName.ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;

                if (value.IsNullOrWhiteSpace()) return false;

                var config = HttpUtility.ParseQueryString(value);

                var expiry = config["expiry"].Parse(TimeSpan.Zero);

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["when"].IsNullOrWhiteSpace())
                {
                    var httpRequestData = functionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                    var variables = functionContext.GetBindingData<string>("variables") ?? "{}";
                    var when = HttpUtility.UrlDecode(config["when"]);

                    var jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            OperationName = operationName,
                            Headers = httpRequestData.Headers.ToDictionary(k => k.Key, v => v.Value),
                            Variables = Json.Deserialize<dynamic>(variables)
                        }))
                    };

                    isCachedEnabled = jArray.SelectToken(when) is not null;
                }

                key = isCachedEnabled ? BuildKey(functionContext) : string.Empty;

                return isCachedEnabled;
            }
            public static bool IsCacheEnabled(HttpResponseMessage response, out string key) => IsCacheEnabled(response.RequestMessage, out key);
            public static bool IsCacheEnabled(HttpRequestMessage request, out string key)
            {
                var value = Find($"Cache:{$"{request.Method}:{request.RequestUri.ToString().Trim('/')}".ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;

                if (value.IsNullOrWhiteSpace()) return false;

                var config = HttpUtility.ParseQueryString(value);

                var expiry = config["expiry"].Parse(TimeSpan.Zero);

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["when"].IsNullOrWhiteSpace())
                {
                    var when = HttpUtility.UrlDecode(config["when"]).Split(',', StringSplitOptions.RemoveEmptyEntries);

                    var jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            Method = request.Method.ToString(),
                            Uri = request.RequestUri,
                            Headers = request.Headers.Select(x => new { x.Key, x.Value }),
                            Content = request.Content.ReadJsonAs<dynamic>()
                        }))
                    };

                    isCachedEnabled = when.All(x => jArray.SelectToken(x) is not null);
                }

                key = isCachedEnabled ? BuildKey(request) : string.Empty;

                return isCachedEnabled;
            }

            public static bool ShouldCache(FunctionContext functionContext, ref string content, out TimeSpan expiry, out bool isSliding)
            {
                expiry = TimeSpan.Zero;
                isSliding = true;
                content = null;

                if (functionContext is null) return false;

                var body = functionContext.GetHttpResponseData().Body;

                body.Position = 0;

                content = new StreamReader(body).ReadToEnd();

                if (content.Contains("\"errors\"", StringComparison.OrdinalIgnoreCase)) return false;

                var operationName = functionContext.GetOperationName();
                var value = Find($"Cache:{operationName.ToLower()}", "^Cache:.*", "expiry=00:00:00");

                if (value.IsNullOrWhiteSpace()) return false;

                var config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = !config["isSliding"].TryParse<bool>(out var result) || result;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["should"].IsNullOrWhiteSpace())
                {
                    var httpRequestData = functionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                    var variables = functionContext.GetBindingData<string>("variables") ?? "{}";
                    var should = HttpUtility.UrlDecode(config["should"]).Split(',', StringSplitOptions.RemoveEmptyEntries);

                    var jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            OperationName = operationName,
                            Headers = httpRequestData.Headers.ToDictionary(k => k.Key, v => v.Value),
                            Variables = Json.Deserialize<dynamic>(variables),
                            Response = Json.Deserialize<dynamic>(content)
                        }))
                    };

                    isCachedEnabled = should.All(x => jArray.SelectToken(x) is not null);
                }

                return isCachedEnabled;
            }
            public static bool ShouldCache(HttpResponseMessage response, out TimeSpan expiry, out bool isSliding)
            {
                expiry = TimeSpan.Zero;
                isSliding = true;

                if (response is null) return false;
                if (!response.IsSuccessStatusCode) return false;

                var value = Find($"Cache:{$"{response.RequestMessage.Method}:{response.RequestMessage.RequestUri.ToString().Trim('/')}".ToLower()}", "^Cache:.*", "expiry=00:00:00");

                if (value.IsNullOrWhiteSpace()) return false;

                var config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = !config["isSliding"].TryParse<bool>(out var result) || result;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["should"].IsNullOrWhiteSpace())
                {
                    var should = HttpUtility.UrlDecode(config["should"]).Split(',', StringSplitOptions.RemoveEmptyEntries);

                    var jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            Method = response.RequestMessage.Method.ToString(),
                            Uri = response.RequestMessage.RequestUri,
                            Headers = response.RequestMessage.Headers.Select(x => new { x.Key, x.Value }),
                            Request = Json.Deserialize<dynamic>(response.RequestMessage.Content.ReadAsString()),
                            Response = Json.Deserialize<dynamic>(response.Content.ReadAsString())
                        }))
                    };

                    isCachedEnabled = should.All(x => jArray.SelectToken(x) is not null);
                }

                return isCachedEnabled;
            }


            private static string BuildKey(string operationName, params string[] tokens)
            {
                if (!tokens?.Any() ?? false) throw new ArgumentOutOfRangeException(nameof(tokens));

                var key = string.Join(':', (new[] { operationName }).Concat(tokens.Where(x => !x.IsNullOrWhiteSpace())));

                key = RegexCompiled.Replace(key, WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions);

                if (key.IsNullOrWhiteSpace() || key.Is(operationName))
                    throw new InvalidOperationException("Unable to generate key.");

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(RegexCompiled.Replace(key, WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions)));
            }
            public static string BuildKey(FunctionContext functionContext)
            {
                var key = string.Empty;

                var httpRequestData = functionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                var operationName = functionContext.GetOperationName();
                var query = functionContext.GetBindingData<string>("query") ?? string.Empty;
                var variables = functionContext.GetBindingData<string>("variables") ?? "{}";
                var config = HttpUtility.ParseQueryString(Find($"Cache:{operationName}", "^Cache:.*", "expiry=00:00:00"));

                var configRequestHeaders = config.AllKeys.Any(k => k.Is("rq_headers")) ? config["rq_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var configResponseHeaders = functionContext.Items.ContainsKey(Constants.Application.ResponseHeaderKey) ? config.AllKeys.Any(k => k.Is("rs_headers")) ? config["rs_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>() : Array.Empty<string>();
                var configGraphQLQuery = config.AllKeys.Any(k => k.Is("query")) ? config["query"] : string.Empty;
                var configGraphQLVariables = config.AllKeys.Any(k => k.Is("variables")) ? config["variables"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var useCustomKey = configRequestHeaders.Any() || configGraphQLQuery.Any() || configGraphQLVariables.Any();

                if (useCustomKey)
                {
                    // Convert headers into query string so we can use a case insensitive collection
                    var requestHeadersCollection = HttpUtility.ParseQueryString(string.Join('&', httpRequestData.Headers.Select(x => $"{x.Key}={string.Join(',', x.Value)}")));
                    var responseHeadersCollection = functionContext.Items.ContainsKey(Constants.Application.ResponseHeaderKey) ? HttpUtility.ParseQueryString(string.Join('&', ((Dictionary<string, string>)functionContext.Items[Constants.Application.ResponseHeaderKey]).Select(x => $"{x.Key}={x.Value}"))) : new NameValueCollection();
                    var variablesObject = JsonConvert.DeserializeObject<JToken>(variables);

                    var requestHeaders = configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                             .Select(x => requestHeadersCollection[x])
                                                             .Where(x => !x.IsNullOrWhiteSpace());
                    var responseHeaders = configResponseHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                             .Select(x => responseHeadersCollection[x])
                                                             .Where(x => !x.IsNullOrWhiteSpace());
                    var requestQuery = !configGraphQLQuery.IsNullOrWhiteSpace() ? RegexCompiled.Match(query, configGraphQLQuery).Value : string.Empty;
                    var requestVariables = configGraphQLVariables.Select(x => variablesObject.SelectToken(x)?.ToString() ?? string.Empty)
                                                                 .Where(x => !x.IsNullOrWhiteSpace());

                    var tokens = new string[5]
                    {
                        operationName,
                        string.Join(',', requestHeaders),
                        string.Join(',', responseHeaders),
                        requestQuery,
                        string.Join(',', requestVariables)
                    };

                    key = RegexCompiled.Replace(string.Join(':', tokens.Where(x => !x.IsNullOrWhiteSpace())), WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions);
                }

                if (key.IsNullOrWhiteSpace() || key.Is(operationName))
                    key = RegexCompiled.Replace($"{Json.Serialize(httpRequestData.Headers)}:{operationName}:{query}:{variables}".ToLower(), WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions);

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
            }
            public static string BuildKey(HttpResponseMessage response) => BuildKey(response.RequestMessage, response);
            public static string BuildKey(HttpRequestMessage request, HttpResponseMessage response = null)
            {
                var key = string.Empty;
                var requestUri = request.RequestUri.ToString().Trim('/').ToLower();
                var keyStart = $"{request.Method}:{requestUri}".ToLower();
                var requestPayload = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "{}";
                var responsePayload = response?.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "{}";
                var config = HttpUtility.ParseQueryString(Find($"Cache:{$"{request.Method}:{requestUri}".ToLower()}", "^Cache:.*", "expiry=00:00:00"));

                if (requestPayload.IsNullOrWhiteSpace())
                    requestPayload = "{}";

                if (responsePayload.IsNullOrWhiteSpace())
                    responsePayload = "{}";

                var configRequestUrl = config.AllKeys.Any(k => k.Is("url")) ? config["url"] : string.Empty;
                var configRequestQueryParameters = config.AllKeys.Any(k => k.Is("params")) ? config["params"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var configRequestHeaders = config.AllKeys.Any(k => k.Is("rq_headers")) ? config["rq_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var configRequestContent = config.AllKeys.Any(k => k.Is("rq_content")) ? config["rq_content"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var configResponseHeaders = config.AllKeys.Any(k => k.Is("rs_headers")) ? config["rs_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var configResponseContent = config.AllKeys.Any(k => k.Is("rs_content")) ? config["rs_content"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                var useCustomKey = !configRequestUrl.IsNullOrWhiteSpace() || configRequestQueryParameters.Any() || configRequestHeaders.Any() || configRequestContent.Any() || response != null && (configResponseHeaders.Any() || configResponseContent.Any());

                if (useCustomKey)
                {
                    var requestUrl = !configRequestUrl.IsNullOrWhiteSpace() ? RegexCompiled.Match(requestUri, configRequestUrl).Value : string.Empty;
                    var requestQueryParams = HttpUtility.ParseQueryString(requestUri);
                    var requestParams = configRequestQueryParameters.Select(x => requestQueryParams[x])
                                                                    .Where(x => !x.IsNullOrWhiteSpace());
                    var requestHeaders = configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                             .Select(x => string.Join(string.Empty, request.Headers.GetValues(x)))
                                                             .Where(x => !x.IsNullOrWhiteSpace());
                    var requestObject = JsonConvert.DeserializeObject<JToken>(requestPayload);
                    var requestContent = configRequestContent.Select(x => requestObject.SelectToken(x)?.ToString() ?? string.Empty)
                                                             .Where(x => !x.IsNullOrWhiteSpace());

                    var responseHeaders = response != null ? configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                                                 .Select(x => string.Join(string.Empty, response.Headers.GetValues(x)))
                                                                                 .Where(x => !x.IsNullOrWhiteSpace())
                                                           : Enumerable.Empty<string>();
                    var responseObject = JsonConvert.DeserializeObject<JToken>(responsePayload);
                    var responseContent = configResponseContent.Select(x => responseObject.SelectToken(x).ToString() ?? string.Empty)
                                                               .Where(x => !x.IsNullOrWhiteSpace());

                    var tokens = new string[6]
                    {
                        !requestUrl.IsNullOrWhiteSpace() ? requestUrl : key,
                        string.Join(',', requestParams),
                        string.Join(',', requestHeaders),
                        string.Join(',', responseHeaders),
                        string.Join(',', requestContent),
                        string.Join(',', responseContent)
                    };

                    key = RegexCompiled.Replace(string.Join(':', tokens.Where(x => !x.IsNullOrWhiteSpace())), WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions);
                }

                if (key.IsNullOrWhiteSpace())
                    key = RegexCompiled.Replace($"{keyStart}:{string.Join(',', request.Headers.Where(x => !ExcludedHeaders.Contains(x.Key)).OrderBy(x => x.Key.ToLower()).Select(x => $"{x.Key}:{string.Join(',', x.Value)}"))}:{request.Content?.ReadAsStringAsync().Result}".ToLower(), WhitespaceRegexPattern, string.Empty, WhitespaceRegexOptions);

                Debug.WriteLine($"Key: {key}");

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(key.Trim()));
            }
        }
    }
}