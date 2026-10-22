using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Serialisation;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class ConfigReader
    {
        private static readonly string[] ExcludedHeaders = new[] { "postman-token" };
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
                        _cache.Value ??= new();
                    }

                    semaphoreSlim.Release();
                }

                return _cache.Value;
            }
        }


        public static string Get(string key, string @default = default) => Try<string>(key, @default);

        public static T Get<T>(string key, T @default = default) => Try<T>(key, @default);

        public static void Set(string key, string value)
        {
            if (key.IsNullOrWhiteSpace())
                throw new ArgumentNullException(key);

            Cache.AddOrUpdate($"Process:{key}", value, (k, v) => value);

            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        }

        public static string Try(string key, string @default = default) => Try<string>(key, @default);

        public static T Try<T>(string key, T @default = default)
        {
            if (key.IsNullOrWhiteSpace())
                return @default;

            string requestedKey = key;
            key = key.ToLowerInvariant();

            string configValue = GetEnvironmentVariable(requestedKey, ignoreCase: false);

            if (configValue.IsNullOrWhiteSpace() && Cache.TryGetValue(key, out object value))
                return (T)value;

            configValue ??= GetEnvironmentVariable(requestedKey, ignoreCase: true);

            if (configValue.IsNullOrWhiteSpace())
                if (!Cache.TryGetValue($"Process:{key}", out value))
                    return @default;
                else
                    configValue = value as string;

            if (configValue.IsNullOrWhiteSpace())
                return @default;

            if (configValue.TryParse(out T result, @default))
                Cache.AddOrUpdate(key, result, (k, v) => result);

            return result ?? @default;
        }

        private static string GetEnvironmentVariable(string key, bool ignoreCase)
        {
            foreach (EnvironmentVariableTarget target in new[]
            {
                EnvironmentVariableTarget.Process,
                EnvironmentVariableTarget.User,
                EnvironmentVariableTarget.Machine
            })
            {
                string value = Environment.GetEnvironmentVariable(key, target);
                if (!value.IsNullOrWhiteSpace())
                    return value;

                if (!ignoreCase)
                    continue;

                var variables = Environment.GetEnvironmentVariables(target);
                string matchingKey = variables.Keys
                    .OfType<string>()
                    .FirstOrDefault(candidate => candidate.Is(key));

                if (!matchingKey.IsNullOrWhiteSpace())
                {
                    value = variables[matchingKey]?.ToString();
                    if (!value.IsNullOrWhiteSpace())
                        return value;
                }
            }

            return null;
        }

        public static string Find(string key, string filter = "") => Find(key, filter, string.Empty);

        public static T Find<T>(string key, string filter = "", T @default = default)
        {
            string[] variables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Keys.OfType<string>().ToArray();

            variables = FilterVariables(filter);

            foreach (string variable in variables)
                if (RegexCompiled.IsMatch(key, HttpUtility.UrlDecode(variable), RegexOptions.Singleline | RegexOptions.IgnoreCase))
                    return Get(variable, @default);

            return @default;
        }

        public static string[] FilterVariables(string filter = "")
        {
            string[] variables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Keys.OfType<string>().ToArray();

            if (!filter.IsNullOrWhiteSpace())
                variables = variables.Where(f => RegexCompiled.IsMatch(f, filter, RegexOptions.Singleline | RegexOptions.IgnoreCase)).ToArray();

            return variables;
        }

        public static void LoadEnvironmentSettingsFile(bool optional = true, bool forceOverride = true, string root = ".", string environmentVariableName = "xunit_environment_config")
        {
            // This is used for local test runs and if we require to run tests locally against specific environments
            // Will require xunit_environment_config, or what ever the value of environmentVariableName being passed,
            // to be set on the environment of the machine.

            string filePath = $"local.settings.{Get(environmentVariableName, "local")}.json";
            string fullPath = !root.IsNullOrWhiteSpace() ? Path.Combine(root, filePath) : Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), filePath);

            if (!File.Exists(fullPath))
            {
                filePath = $"{Get(environmentVariableName, "local")}.settings.json";

                fullPath = !root.IsNullOrWhiteSpace() ? Path.Combine(root, filePath) : Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), filePath);
            }

            if (!File.Exists(fullPath))
                return;

            LoadEnvironmentSettingsFile(filePath, optional, forceOverride, root);
        }

        public static void LoadEnvironmentSettingsFile(string fileName, bool optional = true, bool forceOverride = true, string root = ".")
        {
            if (fileName.IsNullOrWhiteSpace())
                fileName = "local.settings.json";

            string filePath = fileName;

            if (!optional && !root.IsNullOrWhiteSpace() && !Directory.Exists(root))
                throw new DirectoryNotFoundException($"'{root}' not found");

            if (!root.IsNullOrWhiteSpace())
                filePath = Path.Combine(root, filePath);
            else
                filePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), filePath);

            if (optional && !File.Exists(filePath))
                return;
            if (!optional && !File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            string json = File.ReadAllText(filePath);

            JObject values = JObject.Parse(json).Value<JObject>("Values");

            foreach (KeyValuePair<string, JToken?> item in values)
                if (forceOverride || (!forceOverride && Environment.GetEnvironmentVariable(item.Key, EnvironmentVariableTarget.Process) == null))
                    Set(item.Key, item.Value.ToString());
        }

        public static class Caching
        {
            private static readonly Regex whitespaceRegex = new("\\s+", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

            public static bool IsCacheEnabled(string operationName, out string key, out TimeSpan expiry, out bool isSliding, params string[] tokens)
            {
                string value = Find($"Cache:{operationName.ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;
                expiry = TimeSpan.Zero;
                isSliding = true;

                if (value.IsNullOrWhiteSpace())
                    return false;

                NameValueCollection config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = config["isSliding"].TryParse<bool>(out bool result) ? result : true;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                key = isCachedEnabled ? BuildKey(operationName, tokens) : string.Empty;

                return isCachedEnabled;
            }
            public static bool IsCacheEnabled(FunctionContext functionContext, out string key, out TimeSpan expiry, out bool isSliding)
            {
                string operationName = functionContext.GetOperationName();
                string value = Find($"Cache:{operationName.ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;
                expiry = TimeSpan.Zero;
                isSliding = true;

                if (value.IsNullOrWhiteSpace())
                    return false;

                NameValueCollection config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = config["isSliding"].TryParse<bool>(out bool result) ? result : true;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["when"].IsNullOrWhiteSpace())
                {
                    HttpRequestData httpRequestData = functionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                    string variables = functionContext.GetBindingData<string>("variables") ?? "{}";

                    JArray jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            OperationName = operationName,
                            Headers = httpRequestData.Headers.ToDictionary(k => k.Key, v => v.Value),
                            Variables = Json.Deserialize<dynamic>(variables)
                        }))
                    };

                    isCachedEnabled = jArray.SelectToken(config["when"]) is not null;
                }

                key = isCachedEnabled ? BuildKey(functionContext) : string.Empty;

                return isCachedEnabled;
            }
            public static bool IsCacheEnabled(HttpResponseMessage response, out string key, out TimeSpan expiry, out bool isSliding) => IsCacheEnabled(response.RequestMessage, out key, out expiry, out isSliding);
            public static bool IsCacheEnabled(HttpRequestMessage request, out string key, out TimeSpan expiry, out bool isSliding)
            {
                string value = Find($"Cache:{$"{request.Method}:{request.RequestUri.ToString().Trim('/')}".ToLower()}", "^Cache:.*", "expiry=00:00:00");

                key = string.Empty;
                expiry = TimeSpan.Zero;
                isSliding = true;

                if (value.IsNullOrWhiteSpace())
                    return false;

                NameValueCollection config = HttpUtility.ParseQueryString(value);

                expiry = config["expiry"].Parse(TimeSpan.Zero);
                isSliding = config["isSliding"].TryParse<bool>(out bool result) ? result : true;

                bool isCachedEnabled = expiry != TimeSpan.Zero;

                if (isCachedEnabled && !config["when"].IsNullOrWhiteSpace())
                {
                    JArray jArray = new JArray()
                    {
                        Json.Deserialize<JObject>(Json.Serialize(new
                        {
                            Method = request.Method.ToString(),
                            Uri = request.RequestUri,
                            Headers = request.Headers.Select(x => new { x.Key, x.Value }),
                            Content = Json.Deserialize<dynamic>(request.Content.ReadAsStringAsync().GetAwaiter().GetResult())
                        }))
                    };

                    isCachedEnabled = jArray.SelectToken(config["when"]) is not null;
                }

                key = isCachedEnabled ? BuildKey(request) : string.Empty;

                return isCachedEnabled;
            }


            private static string BuildKey(string operationName, params string[] tokens)
            {
                if (!tokens?.Any() ?? false)
                    throw new ArgumentOutOfRangeException(nameof(tokens));

                string key = string.Join(':', (new[] { operationName }).Concat(tokens.Where(x => !x.IsNullOrWhiteSpace())));

                key = whitespaceRegex.Replace(key, string.Empty);

                if (key.IsNullOrWhiteSpace() || key.Is(operationName))
                    throw new InvalidOperationException("Unable to generate key.");

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(whitespaceRegex.Replace(key, string.Empty)));
            }
            public static string BuildKey(FunctionContext functionContext)
            {
                string key = string.Empty;

                HttpRequestData httpRequestData = functionContext.GetHttpRequestDataAsync().GetAwaiter().GetResult();
                string operationName = functionContext.GetOperationName();
                string query = functionContext.GetBindingData<string>("query") ?? string.Empty;
                string variables = functionContext.GetBindingData<string>("variables") ?? "{}";
                NameValueCollection config = HttpUtility.ParseQueryString(Find($"Cache:{operationName}", "^Cache:.*", "expiry=00:00:00"));

                string[] configRequestHeaders = config.AllKeys.Any(k => k.Is("rq_headers")) ? config["rq_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                string[] configResponseHeaders = functionContext.Items.ContainsKey(Constants.Application.ResponseHeaderKey) ? config.AllKeys.Any(k => k.Is("rs_headers")) ? config["rs_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>() : Array.Empty<string>();
                string configGraphQLQuery = config.AllKeys.Any(k => k.Is("query")) ? config["query"] : string.Empty;
                string[] configGraphQLVariables = config.AllKeys.Any(k => k.Is("variables")) ? config["variables"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                bool useCustomKey = configRequestHeaders.Any() || configGraphQLQuery.Any() || configGraphQLVariables.Any();

                if (useCustomKey)
                {
                    // Convert headers into query string so we can use a case insensitive collection
                    NameValueCollection requestHeadersCollection = HttpUtility.ParseQueryString(string.Join('&', httpRequestData.Headers.Select(x => $"{x.Key}={string.Join(',', x.Value)}")));
                    NameValueCollection responseHeadersCollection = functionContext.Items.ContainsKey(Constants.Application.ResponseHeaderKey) ? HttpUtility.ParseQueryString(string.Join('&', ((Dictionary<string, string>)functionContext.Items[Constants.Application.ResponseHeaderKey]).Select(x => $"{x.Key}={x.Value}"))) : new NameValueCollection();
                    JToken variablesObject = JsonConvert.DeserializeObject<JToken>(variables);

                    IEnumerable<string> requestHeaders = configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                                             .Select(x => requestHeadersCollection[x])
                                                                             .Where(x => !x.IsNullOrWhiteSpace());
                    IEnumerable<string> responseHeaders = configResponseHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                                               .Select(x => responseHeadersCollection[x])
                                                                               .Where(x => !x.IsNullOrWhiteSpace());
                    string requestQuery = !configGraphQLQuery.IsNullOrWhiteSpace() ? RegexCompiled.Match(query, configGraphQLQuery).Value : string.Empty;
                    IEnumerable<string> requestVariables = configGraphQLVariables.Select(x => variablesObject.SelectToken(x)?.ToString() ?? string.Empty)
                                                                                                             .Where(x => !x.IsNullOrWhiteSpace());

                    string[] tokens = new string[5]
                    {
                        operationName,
                        string.Join(',', requestHeaders),
                        string.Join(',', responseHeaders),
                        requestQuery,
                        string.Join(',', requestVariables)
                    };

                    key = whitespaceRegex.Replace(string.Join(':', tokens.Where(x => !x.IsNullOrWhiteSpace())), string.Empty);
                }

                if (key.IsNullOrWhiteSpace() || key.Is(operationName))
                    key = whitespaceRegex.Replace($"{Json.Serialize(httpRequestData.Headers)}:{operationName}:{query}:{variables}".ToLower(), string.Empty);

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
            }
            public static string BuildKey(HttpResponseMessage response) => BuildKey(response.RequestMessage, response);
            public static string BuildKey(HttpRequestMessage request, HttpResponseMessage response = null)
            {
                string key = string.Empty;
                string requestUri = request.RequestUri.ToString().Trim('/').ToLower();
                string keyStart = $"{request.Method}:{requestUri}".ToLower();
                string requestPayload = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "{}";
                string responsePayload = response?.Content?.ReadAsStringAsync().GetAwaiter().GetResult() ?? "{}";
                NameValueCollection config = HttpUtility.ParseQueryString(Find($"Cache:{$"{request.Method}:{requestUri}".ToLower()}", "^Cache:.*", "expiry=00:00:00"));

                if (requestPayload.IsNullOrWhiteSpace())
                    requestPayload = "{}";

                if (responsePayload.IsNullOrWhiteSpace())
                    responsePayload = "{}";

                string configRequestUrl = config.AllKeys.Any(k => k.Is("url")) ? config["url"] : string.Empty;
                string[] configRequestQueryParameters = config.AllKeys.Any(k => k.Is("params")) ? config["params"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                string[] configRequestHeaders = config.AllKeys.Any(k => k.Is("rq_headers")) ? config["rq_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                string[] configRequestContent = config.AllKeys.Any(k => k.Is("rq_content")) ? config["rq_content"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                string[] configResponseHeaders = config.AllKeys.Any(k => k.Is("rs_headers")) ? config["rs_headers"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                string[] configResponseContent = config.AllKeys.Any(k => k.Is("rs_content")) ? config["rs_content"].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                bool useCustomKey = !configRequestUrl.IsNullOrWhiteSpace() || configRequestQueryParameters.Any() || configRequestHeaders.Any() || configRequestContent.Any() || response != null && (configResponseHeaders.Any() || configResponseContent.Any());

                if (useCustomKey)
                {
                    string requestUrl = !configRequestUrl.IsNullOrWhiteSpace() ? RegexCompiled.Match(requestUri, configRequestUrl).Value : string.Empty;
                    NameValueCollection requestQueryParams = HttpUtility.ParseQueryString(requestUri);
                    IEnumerable<string> requestParams = configRequestQueryParameters.Select(x => requestQueryParams[x])
                                                                                    .Where(x => !x.IsNullOrWhiteSpace());
                    IEnumerable<string> requestHeaders = configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                                             .Select(x => string.Join(string.Empty, request.Headers.GetValues(x)))
                                                                             .Where(x => !x.IsNullOrWhiteSpace());
                    JToken requestObject = JsonConvert.DeserializeObject<JToken>(requestPayload);
                    IEnumerable<string> requestContent = configRequestContent.Select(x => requestObject.SelectToken(x)?.ToString() ?? string.Empty)
                                                                             .Where(x => !x.IsNullOrWhiteSpace());

                    IEnumerable<string> responseHeaders = response != null ? configRequestHeaders.Where(x => !ExcludedHeaders.Contains(x))
                                                                                                 .Select(x => string.Join(string.Empty, response.Headers.GetValues(x)))
                                                                                                 .Where(x => !x.IsNullOrWhiteSpace())
                                                           : Enumerable.Empty<string>();
                    JToken responseObject = JsonConvert.DeserializeObject<JToken>(responsePayload);
                    IEnumerable<string> responseContent = configResponseContent.Select(x => responseObject.SelectToken(x).ToString() ?? string.Empty)
                                                                               .Where(x => !x.IsNullOrWhiteSpace());

                    string[] tokens = new string[6]
                    {
                        !requestUrl.IsNullOrWhiteSpace() ? requestUrl : key,
                        string.Join(',', requestParams),
                        string.Join(',', requestHeaders),
                        string.Join(',', responseHeaders),
                        string.Join(',', requestContent),
                        string.Join(',', responseContent)
                    };

                    key = whitespaceRegex.Replace(string.Join(':', tokens.Where(x => !x.IsNullOrWhiteSpace())), string.Empty);
                }

                if (key.IsNullOrWhiteSpace())
                    key = whitespaceRegex.Replace($"{keyStart}:{string.Join(',', request.Headers.Where(x => !ExcludedHeaders.Contains(x.Key)).OrderBy(x => x.Key.ToLower()).Select(x => $"{x.Key}:{string.Join(',', x.Value)}"))}:{request.Content?.ReadAsStringAsync().Result}".ToLower(), string.Empty);

                Debug.WriteLine($"Key: {key}");

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(key.Trim()));
            }
        }
    }
}

