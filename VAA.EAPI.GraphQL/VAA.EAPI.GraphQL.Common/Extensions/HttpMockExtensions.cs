using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Http.Mocking;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpMockExtensions
    {
        private const string UriSchemaPattern = @"^([filetphsmaorcnqw]+):/([^/])";

        public static HttpMock? FindMock(this ConcurrentBag<HttpMock> source, HttpRequestMessage request, ILogger logger) => FindMock((IEnumerable<HttpMock>)source, request, logger);
        public static HttpMock? FindMock(this HttpMock[] source, HttpRequestMessage request, ILogger logger) => FindMock((IEnumerable<HttpMock>)source, request, logger);
        private static HttpMock? FindMock(IEnumerable<HttpMock> source, HttpRequestMessage request, ILogger logger)
        {
            if (source.IsNullOrEmpty()) return default;

            var requestUriRaw = request!.RequestUri!.ToString();

#warning fix issue of apim calls stripping out '/' character from requestUri
            requestUriRaw = RegexCompiled.Replace(requestUriRaw, UriSchemaPattern, @"$1://$2");

            var requestUri = requestUriRaw.Parse<Uri>();

            var headerCheckRegex = $@"{request.Method}\s+{Regex.Escape(requestUri.OriginalString)}\s+HTTP/{request.Version}";
            var httpMocks = source.Where(x => RegexCompiled.IsMatch(x.RawRequest, headerCheckRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline));

            var httpMock = httpMocks.FirstOrDefault(x =>
            {
                try
                {
                    var headerMatch = x.Request.Headers.All(x => request.Headers.TryGetValues(x.Key, out var values) && RegexCompiled.Try.IsMatch(new StringValues([.. values]).ToString(), new StringValues([.. x.Value]).ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline, out var isMatch) && isMatch.GetValueOrDefault());
                    var mockRequestContent = x.Request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                    var contentRules = mockRequestContent?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries) ?? [];
                    var contentMatch = !contentRules.Any();

                    if (request.Method.Method.IsAny(HttpMethods.Put, HttpMethods.Post, HttpMethods.Patch, HttpMethods.Delete) && request.Content?.Headers.ContentLength > 0 && contentRules.Length > 0)
                    {
                        var content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim();

                        if (content.IsNullOrWhiteSpace()) return default;

                        var mediaType = request!.Content!.Headers!.ContentType?.ToString().ToLower() ?? string.Empty;

                        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);

                        switch (mediaTypeHeaderValue.MediaType!)
                        {
                            case "application/json":

                                try
                                {
                                    if (content.StartsWith('{'))
                                        content = $"[{content}]";

                                    var requestObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JToken>(content)!;

                                    contentMatch = contentRules.All(r => requestObject.SelectTokens(r).Any());
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching JSON content rules for request {Method} {RequestUri} {Protocol}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                            case "application/xml":
                            case "text/xml":

                                try
                                {
                                    var xmlDocument = new XmlDocument();

                                    xmlDocument.LoadXml(content);

                                    var xmlNamespaceManager = xmlDocument.CreateXmlNamespaceManager();

                                    contentMatch = contentRules.All(x => xmlDocument.SelectSingleNode(x, xmlNamespaceManager) is not null);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching XML content rules for request {Method} {RequestUri} {Protocol}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                            default:

                                try
                                {
                                    contentMatch = contentRules.All(x => RegexCompiled.Try.IsMatch(content, x, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase, out var isMatch) && isMatch.GetValueOrDefault());
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error matching content rules for request {Method} {RequestUri} {Protocol}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                                    contentMatch = false;
                                }

                                break;
                        }
                    }

                    return headerMatch && contentMatch;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error matching mock with request {Method} {RequestUri} {Protocol}. {Message}", request.Method, requestUri, request.RequestUri.Scheme, ex.Message);

                    return false;
                }
            });

            if (httpMock is not null)
            {
                Console.WriteLine($"Mocking {request.Method} {requestUri} {request.RequestUri.Scheme}.");
                Debug.WriteLine($"Mocking {request.Method} {requestUri} {request.RequestUri.Scheme}.");

                return httpMock;
            }

            return default;
        }
    }
}
