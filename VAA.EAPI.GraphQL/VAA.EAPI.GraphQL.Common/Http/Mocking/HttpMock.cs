using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Common.Http.Mocking
{
    public sealed class HttpMock
    {
        private const string XMockRawRequestHeader = "x-mock-raw-request";
        private const string RequestMarker = "###Request";
        private const string ResponseMarker = "###Response";

        public static explicit operator HttpMock(FileInfo source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            if (!source.Exists) return default;

            var httpMock = (HttpMock)File.ReadAllLines(source.FullName);

            httpMock.Id = source.Name;

            return httpMock;
        }
        public static explicit operator HttpMock(string[] source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            return new(source);
        }
        public static explicit operator HttpMock(BlobDownloadResult source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return new(source.Content.ToString().Split(Environment.NewLine));
        }


        private readonly string _rawRequest = string.Empty;
        private readonly string _rawResponse = string.Empty;
        private HttpStatusCode _responseStatusCode;
        private Version _responseVersion = HttpVersion.Version11;
        private string _responseReasonPhrase;
        private byte[] _responseBody = [];
        private readonly Dictionary<string, string[]> _responseHeaders = [];
        private readonly Dictionary<string, string[]> _responseContentHeaders = [];

        public string Id { get; private set; }
        public string[] Contents { get; }
        public HttpRequestMessage Request { get; }
        public HttpResponseMessage Response => CreateResponse(Request);
        public string RawRequest => _rawRequest;
        public string RawResponse => _rawResponse;

        public HttpMock(string[] contents)
        {
            Contents = contents?.Where(x => !x.StartsWith("--")).ToArray() ?? throw new ArgumentNullException(nameof(contents));

            int requestMarkerCount = Contents.Count(x => string.Equals(x, RequestMarker, StringComparison.Ordinal));
            int responseMarkerCount = Contents.Count(x => string.Equals(x, ResponseMarker, StringComparison.Ordinal));

            if (requestMarkerCount != 1) throw new InvalidOperationException($"Expected exactly one {RequestMarker} marker.");
            if (responseMarkerCount != 1) throw new InvalidOperationException($"Expected exactly one {ResponseMarker} marker.");

            int requestPosition = Array.IndexOf(Contents, RequestMarker);
            int responsePosition = Array.IndexOf(Contents, ResponseMarker);

            int requestSectionEnd = requestPosition < responsePosition ? responsePosition : Contents.Length;
            int responseSectionEnd = responsePosition < requestPosition ? requestPosition : Contents.Length;

            string[] requestLines = [.. Contents.Skip(requestPosition + 1).Take(requestSectionEnd - requestPosition - 1)];
            string[] responseLines = [.. Contents.Skip(responsePosition + 1).Take(responseSectionEnd - responsePosition - 1)];

            if (requestLines.Length == 0) throw new InvalidOperationException("Request section is empty.");
            if (responseLines.Length == 0) throw new InvalidOperationException("Response section is empty.");

            Request = CreateRequestMessage(requestLines, out _rawRequest);
            using var response = CreateResponseMessage(responseLines, Request, out _rawResponse);

            CaptureResponse(response);

            if (Request.RequestUri is null) throw new InvalidOperationException("Request URI is missing.");

            Id = Request.RequestUri.ToString();
        }
        public HttpMock(HttpRequestMessage request, HttpResponseMessage response)
        {
            _rawRequest = $"{request.Method} {request.RequestUri} HTTP/{request.Version}{Environment.NewLine}{string.Join(Environment.NewLine, request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").Concat(request.Content?.Headers?.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}") ?? []))}{Environment.NewLine}{(request.Content is not null ? Environment.NewLine + request.Content.ReadAsString() : string.Empty)}";
            _rawResponse = $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}{string.Join(Environment.NewLine, response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}").Concat(response.Content?.Headers?.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}") ?? []))}{Environment.NewLine}{(response.Content is not null ? Environment.NewLine + response.Content.ReadAsString() : string.Empty)}";

            Request = CreateRequestMessage(_rawRequest.Split(Environment.NewLine), out _);
            Id = Request.RequestUri!.ToString();

            CaptureResponse(response);
        }

        public HttpResponseMessage CreateResponse(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = new HttpResponseMessage(_responseStatusCode)
            {
                RequestMessage = request,
                Version = _responseVersion,
                ReasonPhrase = _responseReasonPhrase,
                Content = new ByteArrayContent(_responseBody)
            };

            foreach (var header in _responseHeaders)
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var header in _responseContentHeaders)
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return response;
        }

        private void CaptureResponse(HttpResponseMessage response)
        {
            _responseStatusCode = response.StatusCode;
            _responseVersion = response.Version;
            _responseReasonPhrase = response.ReasonPhrase;
            _responseBody = response.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult() ?? [];

            foreach (var header in response.Headers)
                _responseHeaders[header.Key] = [.. header.Value];

            if (response.Content is null) return;

            foreach (var header in response.Content.Headers)
                _responseContentHeaders[header.Key] = [.. header.Value];
        }

        public static HttpRequestMessage CreateRequestMessage(string request, out string rawHttp)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            rawHttp = request;

            return CreateRequestMessageCore(request.Split(Environment.NewLine));
        }
        public static HttpRequestMessage CreateRequestMessage(string[] requestLines, out string rawHttp)
        {
            if (requestLines is null) throw new ArgumentNullException(nameof(requestLines));

            rawHttp = string.Join(Environment.NewLine, requestLines);

            return CreateRequestMessageCore(requestLines);
        }
        public static HttpRequestMessage CreateRequestMessage(string[] requestLines)
        {
            if (requestLines is null) throw new ArgumentNullException(nameof(requestLines));

            return CreateRequestMessageCore(requestLines);
        }
        private static HttpRequestMessage CreateRequestMessageCore(string[] requestLines)
        {
            if (requestLines.Length == 0) throw new InvalidOperationException("Request section is empty.");

            string requestLine = requestLines[0];

            // GET https://tempuri.org HTTP/1.1
            int firstSpaceIndex = requestLine.IndexOf(' ');

            if (firstSpaceIndex <= 0 || firstSpaceIndex >= requestLine.Length - 1)
                throw new InvalidOperationException("Invalid request line: " + requestLine);

            int secondSpaceIndex = requestLine.IndexOf(' ', firstSpaceIndex + 1);

            if (secondSpaceIndex <= firstSpaceIndex + 1)
                throw new InvalidOperationException("Invalid request line: " + requestLine);

            string method = requestLine[..firstSpaceIndex];
            string requestUri = requestLine[(firstSpaceIndex + 1)..secondSpaceIndex];

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Parse(method), requestUri);

            int contentBreakIndex = -1;

            for (int i = 1; i < requestLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(requestLines[i])) continue;

                contentBreakIndex = i;
                break;
            }

            int contentStartIndex = contentBreakIndex > -1 ? contentBreakIndex + 1 : requestLines.Length;
            string contentText = contentStartIndex < requestLines.Length
                ? string.Join(Environment.NewLine, requestLines, contentStartIndex, requestLines.Length - contentStartIndex)
                : string.Empty;

            httpRequestMessage.Content = new StringContent(contentText);
            httpRequestMessage.Content.Headers.ContentType = null;

            int headerEndIndex = contentBreakIndex > -1 ? contentBreakIndex : requestLines.Length;

            for (int i = 1; i < headerEndIndex; i++)
            {
                string header = requestLines[i];

                int colonOffset = header.IndexOf(':');

                if (colonOffset <= 0) continue;

                string name = header[..colonOffset];
                string value = header[(colonOffset + 1)..].Trim();

                if (!httpRequestMessage.Headers.TryAddWithoutValidation(name, value))
                {
                    bool success = httpRequestMessage.Content.Headers.TryAddWithoutValidation(name, value);

                    System.Diagnostics.Debug.Assert(success, "Failed to add response header: " + header);
                }
            }

            return httpRequestMessage;
        }

        public static HttpResponseMessage CreateResponseMessage(string[] responseLines, HttpRequestMessage request, out string rawHttp)
        {
            if (responseLines is null) throw new ArgumentNullException(nameof(responseLines));

            rawHttp = string.Join(Environment.NewLine, responseLines);

            return CreateResponseMessageCore(responseLines, request);
        }
        public static HttpResponseMessage CreateResponseMessage(string[] responseLines, HttpRequestMessage request)
        {
            if (responseLines is null) throw new ArgumentNullException(nameof(responseLines));

            return CreateResponseMessageCore(responseLines, request);
        }
        private static HttpResponseMessage CreateResponseMessageCore(string[] responseLines, HttpRequestMessage request)
        {
            if (responseLines.Length == 0) throw new InvalidOperationException("Response section is empty.");
            if (request is null) throw new ArgumentNullException(nameof(request));

            string responseLine = responseLines[0];

            // HTTP/1.1 200 OK
            int firstSpaceIndex = responseLine.IndexOf(' ');

            if (firstSpaceIndex <= 0 || firstSpaceIndex >= responseLine.Length - 1)
                throw new InvalidOperationException("Invalid response line: " + responseLine);

            int secondSpaceIndex = responseLine.IndexOf(' ', firstSpaceIndex + 1);

            string statusCodeText = secondSpaceIndex > -1
                ? responseLine.Substring(firstSpaceIndex + 1, secondSpaceIndex - firstSpaceIndex - 1)
                : responseLine[(firstSpaceIndex + 1)..];

            if (!int.TryParse(statusCodeText, NumberStyles.None, CultureInfo.InvariantCulture, out int statusCode))
                throw new InvalidOperationException("Invalid status code: " + statusCodeText);

            HttpResponseMessage httpResponseMessage = new((HttpStatusCode)statusCode)
            {
                RequestMessage = request
            };

            if (secondSpaceIndex > -1 && secondSpaceIndex + 1 < responseLine.Length)
                httpResponseMessage.ReasonPhrase = responseLine[(secondSpaceIndex + 1)..];

            int contentBreakIndex = -1;

            for (int i = 1; i < responseLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(responseLines[i])) continue;

                contentBreakIndex = i;
                break;
            }

            int contentStartIndex = contentBreakIndex > -1 ? contentBreakIndex + 1 : responseLines.Length;
            string contentText = contentStartIndex < responseLines.Length
                ? string.Join(Environment.NewLine, responseLines, contentStartIndex, responseLines.Length - contentStartIndex)
                : string.Empty;

            httpResponseMessage.Content = new StringContent(contentText);
            httpResponseMessage.Content.Headers.ContentType = null;

            int headerEndIndex = contentBreakIndex > -1 ? contentBreakIndex : responseLines.Length;

            for (int i = 1; i < headerEndIndex; i++)
            {
                string header = responseLines[i];

                int colonOffset = header.IndexOf(':');

                if (colonOffset <= 0) continue;

                string name = header[..colonOffset];
                string value = header[(colonOffset + 1)..].Trim();

                if (!httpResponseMessage.Headers.TryAddWithoutValidation(name, value))
                {
                    bool success = httpResponseMessage.Content.Headers.TryAddWithoutValidation(name, value);

                    System.Diagnostics.Debug.Assert(success, "Failed to add response header: " + header);
                }
            }

            return httpResponseMessage;
        }

#nullable enable
        public static HttpRequestMessage ExtractMockRequest(HttpResponseMessage response)
        {
            if (TryExtractMockRequest(response, out var request))
                return request;

            throw new InvalidOperationException($"Mock response did not contain a valid {XMockRawRequestHeader} header.");
        }
        public static bool TryExtractMockRequest(HttpResponseMessage response, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out HttpRequestMessage? request)
        {
            request = null;

            var xMockRawRequestHeader = response.Headers.TryGetValues(XMockRawRequestHeader, out var rawRequestValues) ? string.Join(string.Empty, rawRequestValues) : string.Empty;

            if (xMockRawRequestHeader.IsNullOrWhiteSpace())
                return false;

            try
            {
                var mockRequestContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(xMockRawRequestHeader));

                if (mockRequestContent.IsNullOrWhiteSpace())
                    return false;

                request = CreateRequestMessage(mockRequestContent, out _);

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
#nullable restore
    }
}
