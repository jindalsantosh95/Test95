using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Common.Http
{
    public sealed class LocationDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly HttpStatusCode[] statusCodes;
        private readonly object @lock = new();
        private HttpMessageHandler httpMessageHandler = default;

        public int MaxAutomaticRetries
        {
            get
            {
                if (httpMessageHandler is null)
                    lock (@lock)
                        if (httpMessageHandler is null)
                        {
                            HttpMessageHandler current = InnerHandler;

                            while (current is not HttpClientHandler)
                            {
                                if (current is not DelegatingHandler) break;

                                current = ((DelegatingHandler)current).InnerHandler;

                                if (current is null) break;
                            }

                            if (current is HttpClientHandler)
                                httpMessageHandler = (HttpClientHandler)current;

                            httpMessageHandler ??= new HttpClientHandler();
                        }

                return ((HttpClientHandler)httpMessageHandler).MaxAutomaticRedirections;
            }
        }

        public LocationDelegatingHandler(ILogger logger, params HttpStatusCode[] statusCodes) : base()
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.statusCodes = statusCodes ?? new[] { HttpStatusCode.Accepted };
        }
        public LocationDelegatingHandler(HttpMessageHandler innerHandler, ILogger logger, params HttpStatusCode[] statusCodes) : base(innerHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.statusCodes = statusCodes ?? new[] { HttpStatusCode.Accepted };
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = base.Send(request, cancellationToken);
            var noWait = request.Headers.TryGetValues("x-nowait", out var values) && string.Join(string.Empty, values).Parse<bool>();

            if (noWait) return response;

            var isLocationFromRequest = request.Headers.Contains("Location");
            var isRetryAfterFromRequest = request.Headers.Contains("Retry-After");

            int retries = MaxAutomaticRetries;

            while (statusCodes.Any(x => x.Equals(response.StatusCode)) && retries > 0)
            {
                retries--;

                var location = response.Headers.Location ?? (request.Headers.TryGetValues("Location", out values) ? string.Join(',', values).Parse<Uri>() : default);
                var retryAfter = response.Headers.RetryAfter ?? (request.Headers.TryGetValues("Retry-After", out values) ? RetryConditionHeaderValue.TryParse(string.Join(',', values), out var prsedValue) ? prsedValue : default : default);

                if (location == null) return response;

                if (isLocationFromRequest || isRetryAfterFromRequest)
                {
                    string content = null;

                    if (isLocationFromRequest)
                    {
                        var newLocation = location.ToString();
                        var parameters = RegexCompiled.Matches(location.ToString(), @"{\([^]+)\}")
                                                      .Select(x => x.Groups[1].Value)
                                                      .ToArray();

                        if (parameters.Any())
                        {
                            content ??= response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            if (response.Content.Headers.ContentType.MediaType.Is("application/json"))
                            {
                                var contentObject = Serialisation.Json.Deserialize<JToken>(content);

                                foreach (var parameter in parameters)
                                {
                                    var value = contentObject.SelectToken($"$..{parameter}")?.ToString() ?? string.Empty;

                                    newLocation = newLocation.Replace($"{{{parameter}}}", value);
                                }
                            }
                        }

                        location = newLocation.Parse<Uri>();
                    }

                    if (retryAfter is null && isRetryAfterFromRequest)
                    {
                        var newRetryAfter = string.Join(' ', request.Headers.GetValues("Retry-After"));
                        var parameters = RegexCompiled.Matches(newRetryAfter.ToString(), @"{\([^]+)\}")
                                                      .Select(x => x.Groups[1].Value)
                                                      .ToArray();

                        if (parameters.Any())
                        {
                            content ??= response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            if (response.Content.Headers.ContentType.MediaType.Is("application/json"))
                            {
                                var contentObject = Serialisation.Json.Deserialize<JToken>(content);

                                foreach (var parameter in parameters)
                                {
                                    var value = contentObject.SelectToken($"$..{parameter}")?.ToString() ?? string.Empty;

                                    newRetryAfter = newRetryAfter.Replace($"{{{parameter}}}", value);
                                }
                            }
                        }

                        if (RetryConditionHeaderValue.TryParse(newRetryAfter, out var parsedValue))
                            retryAfter = parsedValue;
                    }
                }

                var waitFor = ConfigReader.Try(Constants.Environment.HttpClientTimeBetweenCalls, 50);

                if (retryAfter is not null)
                {
                    waitFor = (int)(retryAfter.Delta ?? DateTime.Now - (retryAfter.Date ?? DateTime.Now)).TotalMilliseconds;

                    if (waitFor == 0)
                        waitFor = ConfigReader.Try(Constants.Environment.HttpClientTimeBetweenCalls, 50);
                }

                if (response.Headers.Contains("Retry-After") || isRetryAfterFromRequest)
                    Thread.Sleep(waitFor);

                var newRequest = request.Clone(location);

                newRequest.Method = HttpMethod.Get;

                response = base.Send(newRequest, cancellationToken);
            }

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var noWait = request.Headers.TryGetValues("x-nowait", out var values) && string.Join(string.Empty, values).Parse<bool>();

            if (noWait) return response;

            var isLocationFromRequest = request.Headers.Contains("Location");
            var isRetryAfterFromRequest = request.Headers.Contains("Retry-After");

            int retries = MaxAutomaticRetries;

            while (statusCodes.Any(x => x.Equals(response.StatusCode)) && retries > 0)
            {
                retries--;

                var location = response.Headers.Location ?? (request.Headers.TryGetValues("Location", out values) ? string.Join(',', values).Parse<Uri>() : default);
                var retryAfter = response.Headers.RetryAfter ?? (request.Headers.TryGetValues("Retry-After", out values) ? RetryConditionHeaderValue.TryParse(string.Join(',', values), out var prsedValue) ? prsedValue : default : default);

                if (location == null) return response;

                if (isLocationFromRequest || isRetryAfterFromRequest)
                {
                    string content = null;

                    if (isLocationFromRequest)
                    {
                        var newLocation = location.ToString();
                        var parameters = RegexCompiled.Matches(location.ToString(), @"{([^}]+)}")
                                                      .Select(x => x.Groups[1].Value)
                                                      .ToArray();

                        if (parameters.Any())
                        {
                            content ??= response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            if (response.Content.Headers.ContentType.MediaType.Is("application/json"))
                            {
                                var contentObject = Serialisation.Json.Deserialize<JToken>(content);

                                foreach (var parameter in parameters)
                                {
                                    var value = contentObject.SelectToken($"$..{parameter}")?.ToString() ?? string.Empty;

                                    newLocation = newLocation.Replace($"{{{parameter}}}", value);
                                }
                            }
                        }

                        location = newLocation.Parse<Uri>();
                    }

                    if (retryAfter is null && isRetryAfterFromRequest)
                    {
                        var newRetryAfter = string.Join(' ', request.Headers.GetValues("Retry-After"));
                        var parameters = RegexCompiled.Matches(newRetryAfter.ToString(), @"{([^}]+)}")
                                                      .Select(x => x.Groups[1].Value)
                                                      .ToArray();

                        if (parameters.Any())
                        {
                            content ??= response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            if (response.Content.Headers.ContentType.MediaType.Is("application/json"))
                            {
                                var contentObject = Serialisation.Json.Deserialize<JToken>(content);

                                foreach (var parameter in parameters)
                                {
                                    var value = contentObject.SelectToken($"$..{parameter}")?.ToString() ?? string.Empty;

                                    newRetryAfter = newRetryAfter.Replace($"{{{parameter}}}", value);
                                }
                            }
                        }

                        if (RetryConditionHeaderValue.TryParse(newRetryAfter, out var parsedValue))
                            retryAfter = parsedValue;
                    }
                }

                var waitFor = ConfigReader.Try(Constants.Environment.HttpClientTimeBetweenCalls, 50);

                if (retryAfter is not null)
                    waitFor = (int)(retryAfter.Delta ?? DateTime.Now - (retryAfter.Date ?? DateTime.Now)).TotalMilliseconds;

                if (waitFor == 0)
                    waitFor = ConfigReader.Try(Constants.Environment.HttpClientTimeBetweenCalls, 50);

                if (response.Headers.Contains("Retry-After") || isRetryAfterFromRequest)
                    await Task.Delay(waitFor, cancellationToken);

                var newRequest = request.Clone(location);

                newRequest.Method = HttpMethod.Get;

                response = await base.SendAsync(newRequest, cancellationToken);
            }

            return response;
        }
    }
}
