using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;

namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{
    public sealed class RetrieveDocumentService(IHttpConnector httpConnector, IOAuthTokenProvider oauthTokenProvider, ILogger<RetrieveDocumentService> logger) : IRetrieveDocumentService
    {
        private const string UrlConfig = "Holidays:RetrieveDocumentService:Url";
        private const string EntraScopeConfig = "Holidays:EntraScope";
        private const string AuthorizationProviderConfig = "Holidays:AuthorizationProvider";
        private const string TbxUserIdClaimType = "tbx-UserId";
        private const string AllowedHostsConfig = "Holidays:DocumentDownloadAllowedHosts";
        private const string MaximumSizeConfig = "Holidays:DocumentDownloadMaximumBytes";
        private const string TimeoutSecondsConfig = "Holidays:DocumentDownloadTimeoutSeconds";
        private const int DefaultMaximumDocumentSize = 5_242_880;
        private const int DefaultTimeoutSeconds = 30;

        private const string AuthorizationProviderHeader = "Authorization-Provider";
        private const string AuthorizationHeader = "Authorization";
        private const string TBXWebUserIdHeader = "tbx-web-user-id";

        private readonly string AuthorizationProviderHeaderValue = ConfigReader.Try(AuthorizationProviderConfig, "AZURE");

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly IOAuthTokenProvider _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
        private readonly ILogger<RetrieveDocumentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        internal static int NormalizeTimeoutSeconds(int configuredTimeoutSeconds) =>
            configuredTimeoutSeconds > 0 ? configuredTimeoutSeconds : DefaultTimeoutSeconds;

        public async Task<Domain.Documents.HolidaysDocumentListResponse> Handle(RetrieveDocumentRequest request, CancellationToken cancellationToken)
        {
            var scope = $"{ConfigReader.Get(EntraScopeConfig).TrimEnd('/')}/{ConfigReader.Get(Common.Constants.Environment.EapiMsClientID)}";
            var token = await _oauthTokenProvider.AcquireTokenAsync(scope);
            var tbxUserId = token.ExtractJwtClaim(TbxUserIdClaimType);
            var url = $"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/documents/{request.BookingReference}?expand=all&userId={tbxUserId}&cliId=-1";
            var headers = new Dictionary<string, string>
            {
                { AuthorizationProviderHeader, AuthorizationProviderHeaderValue },
                { AuthorizationHeader, token },
                { TBXWebUserIdHeader, tbxUserId.ToString() }
            };

            var response = await _httpConnector.SendAsync(uri: url, headers: headers, bearerToken: null, method: HttpMethod.Get, content: string.Empty);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jObjectTboxResponse = Json.Deserialize<JObject>(content);
                return (Domain.Documents.HolidaysDocumentListResponse)jObjectTboxResponse;
            }

            var exception = new InvalidOperationException($"Failed to retrieve holiday booking document list. Status Code: {response.StatusCode}, Request Booking Id: {request.BookingReference}, CorrelationId: {AsyncContext.Current.CorrelationId}, Response: {content}");

            _logger.LogError(exception, exception.Message);

            return default;
        }

        public async Task<Domain.Documents.HolidaysPendingDocumentListResponse> RetrievePendingDocumentList(RetrieveDocumentRequest retrieveDocumentRequest, CancellationToken cancellationToken)
        {
            var scope = $"{ConfigReader.Get(EntraScopeConfig).TrimEnd('/')}/{ConfigReader.Get(Common.Constants.Environment.EapiMsClientID)}";
            var token = await _oauthTokenProvider.AcquireTokenAsync(scope);
            var tbxUserId = token.ExtractJwtClaim(TbxUserIdClaimType);
            var url = $"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/pending-documents/{retrieveDocumentRequest.BookingReference}?expand=all&userId={tbxUserId}&cliId=-1";

            var headers = new Dictionary<string, string>
            {
                { AuthorizationProviderHeader, AuthorizationProviderHeaderValue },
                { AuthorizationHeader, token },
                { TBXWebUserIdHeader, tbxUserId.ToString() }
            };

            var response = await _httpConnector.SendAsync(uri: url, headers: headers, bearerToken: null, method: HttpMethod.Get, content: string.Empty);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jObjectTboxResponse = Json.Deserialize<JObject>(content);
                return (Domain.Documents.HolidaysPendingDocumentListResponse)(jObjectTboxResponse);
            }

            var exception = new InvalidOperationException($"Failed to retrieve holiday booking pending documents. Status Code: {response.StatusCode}, Request Booking Id: {retrieveDocumentRequest.BookingReference}, CorrelationId: {AsyncContext.Current.CorrelationId}, Response: {content}");

            _logger.LogError(exception, exception.Message);

            return default;
        }

        public async Task<HolidaysDocumentLinkItem> RetrieveDocumentDownloadLink(DocumentLinkRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.DocumentId);

            try
            {
                var scope = $"{ConfigReader.Get(EntraScopeConfig).TrimEnd('/')}/{ConfigReader.Get(Common.Constants.Environment.EapiMsClientID)}";
                var token = await _oauthTokenProvider.AcquireTokenAsync(scope);
                var tbxUserId = token.ExtractJwtClaim(TbxUserIdClaimType);
                var url = $"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/documents/{request.DocumentId}/view-info?expand=all&userId={tbxUserId}&cliId=-1";

                var headers = new Dictionary<string, string>
            {
                { AuthorizationProviderHeader, AuthorizationProviderHeaderValue },
                { AuthorizationHeader, token },
                { TBXWebUserIdHeader, tbxUserId.ToString() }
            };

                var response = await _httpConnector.SendAsync(uri: url, headers: headers, bearerToken: null, method: HttpMethod.Get, content: string.Empty);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var jObjectTboxResponse = JObject.Parse(content);

                if (response.IsSuccessStatusCode)
                {
                    var downloadResponse = (HolidaysDocumentDownloadResponse)(jObjectTboxResponse, request.DocumentId);
                    return (HolidaysDocumentLinkItem)(request, downloadResponse);
                }

                var statusCode = jObjectTboxResponse["status"]?["code"]?.Value<int>();
                string errorMessage = null;
                if (statusCode == -1)
                {
                    var error = jObjectTboxResponse["error"]?["message"]?.Value<string>();
                    errorMessage = error;
                }

                _logger.LogError("Failed to retrieve holiday document download link. Status Code: {StatusCode}, Request Document id: {DocumentId}, CorrelationId: {CorrelationId}, Response: {Response}. Error: {ErrorMessage}", response.StatusCode, request.DocumentId, AsyncContext.Current.CorrelationId, content, errorMessage);

                var errorResponse = new HolidaysDocumentDownloadResponse
                {
                    Errors =
                    [
                        new ErrorDetails
                        {
                            Code = statusCode != null ? $"{statusCode}" : "UnknownError",
                            Message = $"CorrelationId: {AsyncContext.Current.CorrelationId} - {errorMessage ?? $"Failed to retrieve holiday document download link for document id: {request.DocumentId}"}",
                            DeveloperMessage = $"Failed to retrieve holiday document download link for document id: {request.DocumentId} | TimeStamp: {DateTime.UtcNow}",
                            ErrorSource = "Source System"
                        }
                    ]
                };

                return (HolidaysDocumentLinkItem)(request, errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve holiday document download link. Request Document id: {request?.DocumentId}, CorrelationId: {AsyncContext.Current.CorrelationId}");

                var errorResponse = new HolidaysDocumentDownloadResponse
                {
                    Errors =
                    [
                        new ErrorDetails
                        {
                            Code = "UnknownError",
                            Message = $"Error Message: {ex.Message}, CorrelationId: {AsyncContext.Current.CorrelationId} - Failed to retrieve holiday document download link for document id: {request?.DocumentId}",
                            DeveloperMessage = $"Failed to retrieve holiday document download link for document id: {request?.DocumentId} | TimeStamp: {DateTime.UtcNow}",
                            ErrorSource = "Source System"
                        }
                    ]
                };

                return (HolidaysDocumentLinkItem)(request, errorResponse);
            }
        }

        public async Task<HolidaysDocumentLinkListResponse> RetrieveDocumentDownloadLinkList(RetrieveDocumentLinkListRequest request, CancellationToken cancellationToken)
        {
            if (request?.DocumentLinkRequests?.Any() != true)
                return null;

            var requests = request.DocumentLinkRequests.Where(d => d?.DocumentId != null).ToList();
            if (requests.Count == 0) return null;
            const int maxConcurrency = 8;
            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            var tasks = requests.Select(async detail =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await RetrieveDocumentDownloadLink(detail, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return new HolidaysDocumentLinkListResponse
            {
                HolidaysDocumentLinkResponses = results
            };
        }

#nullable enable annotations
        public async Task<RetrieveDocumentDownloadResponse?> RetrieveDocument(RetrieveDocumentDownloadRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.BookingId.IsNullOrWhiteSpace() || request.DocumentId.IsNullOrWhiteSpace() || request.DocumentUrl.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Validated document context is required.", nameof(request));
            }

            if (!Uri.TryCreate(request.DocumentUrl, UriKind.Absolute, out var documentUri) ||
                !documentUri.Scheme.Is(Uri.UriSchemeHttps) ||
                !documentUri.UserInfo.IsNullOrWhiteSpace() ||
                IPAddress.TryParse(documentUri.Host, out var address) && (IPAddress.IsLoopback(address) || IsPrivateAddress(address)) ||
                !IsAllowedDocumentHost(documentUri))
            {
                throw new InvalidOperationException("The validated document URL is not an allowed download target.");
            }

            var documentFileName = GetSafeFileName(documentUri);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var configuredTimeoutSeconds = ConfigReader.Try(TimeoutSecondsConfig, DefaultTimeoutSeconds);
            var timeoutSeconds = NormalizeTimeoutSeconds(configuredTimeoutSeconds);
            timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, documentUri);
            downloadRequest.SuppressRequestUriLogging();
            using var response = await _httpConnector.SendAsync(downloadRequest, timeout.Token);
            if (IsMissingDocumentResponse(response.StatusCode))
            {
                return null;
            }

            if (!response.IsSuccessStatusCode || response.RequestMessage?.RequestUri != documentUri)
            {
                throw new InvalidOperationException("The document download was unavailable or redirected.");
            }

            var maximumSize = ConfigReader.Try(MaximumSizeConfig, DefaultMaximumDocumentSize);
            if (response.Content.Headers.ContentLength > maximumSize ||
                response.Content.Headers.ContentType?.MediaType.Is("application/pdf") != true)
            {
                throw new InvalidOperationException("The document does not meet the approved download policy.");
            }

            await using var source = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var destination = response.Content.Headers.ContentLength is long contentLength
                ? new MemoryStream((int)contentLength)
                : new MemoryStream();
            var buffer = new byte[81920];
            int read;
            while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), timeout.Token)) > 0)
            {
                if (destination.Length + read > maximumSize)
                {
                    throw new InvalidOperationException("The document exceeds the approved maximum size.");
                }

                await destination.WriteAsync(buffer.AsMemory(0, read), timeout.Token);
            }

            var content = destination.ToArray();
            if (content.Length < 5 || !System.Text.Encoding.ASCII.GetString(content, 0, 5).Is("%PDF-"))
            {
                throw new InvalidOperationException("The document is not a PDF.");
            }

            return new RetrieveDocumentDownloadResponse
            {
                DocumentId = request.DocumentId,
                Document = content,
                ContentType = "application/pdf",
                FileName = documentFileName
            };
        }

#nullable restore annotations

        private static string GetSafeFileName(Uri documentUri)
        {
            if (documentUri.AbsolutePath.EndsWith('/'))
            {
                throw new ArgumentException("The validated document URL must contain a filename.", nameof(documentUri));
            }

            var candidate = Uri.UnescapeDataString(documentUri.Segments[^1])
                .Replace("/", string.Empty)
                .Replace("\\", string.Empty);
            candidate = new string(candidate.Where(character => !char.IsControl(character)).Take(128).ToArray());

            if (candidate.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The validated document URL must contain a filename.", nameof(documentUri));
            }

            return candidate;
        }

        private static bool IsAllowedDocumentHost(Uri documentUri)
        {
            var allowedHosts = ConfigReader.Try(AllowedHostsConfig, string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var hostAndPort = $"{documentUri.Host}:{documentUri.Port}";
            return allowedHosts.Any(host => host.Is(hostAndPort) ||
                                            (documentUri.IsDefaultPort && host.Is(documentUri.Host)));
        }

        private static bool IsMissingDocumentResponse(HttpStatusCode statusCode) => statusCode == HttpStatusCode.NotFound;

        private static bool IsPrivateAddress(IPAddress address)
        {
            if (address.IsIPv4MappedToIPv6)
            {
                return IsPrivateAddress(address.MapToIPv4());
            }

            var bytes = address.GetAddressBytes();
            if (bytes.Length == 16)
            {
                return address.Equals(IPAddress.IPv6Any) ||
                       address.Equals(IPAddress.IPv6Loopback) ||
                       address.IsIPv6LinkLocal ||
                       address.IsIPv6Multicast ||
                       (bytes[0] & 0xfe) == 0xfc;
            }

            return bytes[0] == 0 ||
                   bytes[0] == 10 ||
                   bytes[0] == 100 && bytes[1] is >= 64 and <= 127 ||
                   bytes[0] == 127 ||
                   bytes[0] == 169 && bytes[1] == 254 ||
                   bytes[0] == 172 && bytes[1] is >= 16 and <= 31 ||
                   bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0 ||
                   bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2 ||
                   bytes[0] == 192 && bytes[1] == 168 ||
                   bytes[0] == 198 && bytes[1] is 18 or 19 ||
                   bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100 ||
                   bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113 ||
                   bytes[0] >= 224;
        }
    }
}
