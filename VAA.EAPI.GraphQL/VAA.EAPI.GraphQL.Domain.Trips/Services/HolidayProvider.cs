using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;


namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public sealed class HolidayProvider(IHttpConnector httpConnector) : IHolidayProvider
    {
        private const string OcpHeader = "Ocp-Apim-Subscription-Key";
        private const string UrlConfig = "TripSettings:HolidayProvider:HolidaysMs:Url";
        private const string OCPConfig = "TripSettings:HolidayProvider:HolidaysMs:OcpSubID";

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<TripsDetailsResponse> RetrieveBooking(Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest request)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };

            var response = await _httpConnector.SendAsync<TripsDetailsResponse>($"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrieveBooking", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Response;
        }

        public async Task<TripListResponseStore> RetrieveBookingList(IEnumerable<Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest> request)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };

            var response = await _httpConnector.SendAsync<TripListResponseStore>($"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrieveBookingList", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Response;
        }

        public async Task<HolidaysDocumentListResponse> RetrieveDocumentList(RetrieveDocumentRequest request)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };

            var response = await _httpConnector.SendAsync<HolidaysDocumentListResponse>($"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrieveDocumentList", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Response;
        }

        public async Task<HolidaysPendingDocumentListResponse> RetrievePendingDocumentList(RetrieveDocumentRequest request)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };

            var response = await _httpConnector.SendAsync<HolidaysPendingDocumentListResponse>($"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrievePendingDocumentList", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Response;
        }

        public async Task<HolidaysDocumentLinkListResponse> RetrieveDocumentDownloadLinkList(RetrieveDocumentLinkListRequest request)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };

            var response = await _httpConnector.SendAsync<HolidaysDocumentLinkListResponse>($"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrieveDocumentDownloadLinkList", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            return response.Response;
        }

#nullable enable annotations
        public async Task<RetrieveDocumentDownloadResponse?> RetrieveDocument(RetrieveDocumentDownloadRequest request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string> { { OcpHeader, ConfigReader.Get(OCPConfig) } };
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{ConfigReader.Get(UrlConfig).TrimEnd('/')}/retrieveDocument")
            {
                Content = new StringContent(Json.Serialize(request), System.Text.Encoding.UTF8, "application/json")
            };
            message.Headers.Add(OcpHeader, headers[OcpHeader]);
            using var response = await _httpConnector.SendAsync(message, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Holiday document retrieval failed.", null, response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Json.Deserialize<RetrieveDocumentDownloadResponse>(content);
        }
#nullable restore annotations
    }
}
