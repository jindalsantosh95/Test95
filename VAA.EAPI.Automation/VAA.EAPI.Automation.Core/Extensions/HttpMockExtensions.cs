using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Http.Mocking;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class HttpMockExtensions
    {
        private const string UriSchemaPattern = @"^([filetphsmaorcnqw]+):/([^/])";

        public static HttpMock? FindMock(this ConcurrentBag<HttpMock> source, HttpRequestMessage request, ILogger logger) => FindMock((IEnumerable<HttpMock>)source, request, logger);
        public static HttpMock? FindMock(this HttpMock[] source, HttpRequestMessage request, ILogger logger) => FindMock((IEnumerable<HttpMock>)source, request, logger);
        private static HttpMock? FindMock(IEnumerable<HttpMock> source, HttpRequestMessage request, ILogger logger)
        {
            if (source.IsNullOrEmpty())
                return default;

            string requestUriRaw = request!.RequestUri!.ToString();

            // TODO: APIM calls may strip a '/' from the scheme separator in requestUri; normalise it back to "://".
            requestUriRaw = RegexCompiled.Replace(requestUriRaw, UriSchemaPattern, @"$1://$2");

            Uri requestUri = requestUriRaw.Parse<Uri>();
            string requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult().Trim();
            string headerCheckRegex = $@"{request.Method}\s+{Regex.Escape(requestUri.OriginalString)}\s+HTTP/{request.Version}";
            HttpMock[] httpMocks = source.Where(x => RegexCompiled.IsMatch(x.RawRequest, headerCheckRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                                  .ToArray();

            HttpMock httpMock = httpMocks.FirstOrDefault(x =>
            {
                try
                {
                    bool headerMatch = x.Request.Headers.All(x => request.Headers.TryGetValues(x.Key, out IEnumerable<string>? values) && RegexCompiled.Try.IsMatch(new StringValues([.. values]).ToString(), new StringValues([.. x.Value]).ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline, out bool? isMatch) && isMatch.GetValueOrDefault());
                    string mockRequestContent = x.Request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                    string[] contentRules = mockRequestContent?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries) ?? [];
                    bool contentMatch = !contentRules.Any();

                    if (request.Method.Method.IsAny(HttpMethods.Put, HttpMethods.Post, HttpMethods.Patch, HttpMethods.Delete) && request.Content is not null && contentRules.Length > 0)
                    {
                        if (requestContent.IsNullOrWhiteSpace())
                            return default;

                        string mediaType = request.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();

                        switch (mediaType)
                        {
                            case "application/json":

                                try
                                {
                                    string jsonContent = requestContent.Trim();

                                    if (jsonContent.StartsWith('{'))
                                        jsonContent = $"[{jsonContent}]";

                                    JToken requestObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JToken>(jsonContent)!;

                                    contentMatch = contentRules.All(r => requestObject.SelectTokens(r).Any());
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching JSON content rules for request {Method} {RequestUri} {Scheme}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                            case "application/xml":
                            case "text/xml":

                                try
                                {
                                    XmlDocument xmlDocument = new XmlDocument();

                                    xmlDocument.LoadXml(requestContent);

                                    XmlNamespaceManager xmlNamespaceManager = xmlDocument.CreateXmlNamespaceManager();

                                    contentMatch = contentRules.All(x => xmlDocument.SelectSingleNode(x, xmlNamespaceManager) is not null);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching XML content rules for request {Method} {RequestUri} {Scheme}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                            default:

                                try
                                {
                                    contentMatch = contentRules.All(x => RegexCompiled.Try.IsMatch(requestContent, x, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase, out bool? isMatch) && isMatch.GetValueOrDefault());
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching content rules for request {Method} {RequestUri} {Scheme}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                        }
                    }

                    return headerMatch && contentMatch;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error matching mock with request {Method} {RequestUri} {Scheme}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                    return false;
                }
            });

            if (httpMock is not null)
            {
                Console.WriteLine($"Mocking {request.Method} {requestUri}.");
                Debug.WriteLine($"Mocking {request.Method} {requestUri}.");

                return httpMock;
            }

            return default;
        }
    }
}
