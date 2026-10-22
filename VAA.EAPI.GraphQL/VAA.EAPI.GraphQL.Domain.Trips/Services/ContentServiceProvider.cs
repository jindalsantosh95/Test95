using System.ComponentModel.DataAnnotations;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Content;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class ContentServiceProvider : IContentServiceProvider
    {
        protected readonly IHttpConnector _connector;

        public ContentServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<string>> GetContentDetails(string? contentFragmentId, ContentRequest? contentRequest)
        {
            var headers = new Dictionary<string, string>
                            {
                            { Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.ContentServiceOcp) }
                            };
            string baseurl = ConfigReader.Get(Common.Constants.Environment.ContentServiceUrl).TrimEnd('/');

            if (!string.IsNullOrWhiteSpace(contentFragmentId) && (contentRequest?.SourceType is null or SourceType.AEM))
            {
                var uri = $"{baseurl}/content/{Uri.EscapeDataString(contentFragmentId)}";

                var response = await _connector.GetAsync<IEnumerable<string>>(uri, headers, string.Empty);

                return response.Response;
            }

            ValidateContentRequest(contentRequest);

            var result = await _connector.SendAsync<IEnumerable<string>>($"{baseurl}/content", headers, string.Empty, HttpMethod.Post, Json.Serialize(contentRequest));

            return result.Response;
        }

        private static void ValidateContentRequest(ContentRequest? contentRequest)
        {
            if (contentRequest is null)
                throw new RequestValidationException("ContentRequest is required.");

            if (contentRequest.Detail is null)
                throw new RequestValidationException("Detail is required on ContentRequest.");

            if (!string.IsNullOrWhiteSpace(contentRequest.Detail.ContentFragmentName) && (contentRequest.SourceType is null or SourceType.AEM))
            {
                contentRequest.SourceType ??= SourceType.AEM;
                return;
            }

            if (contentRequest.SourceType is null || !Enum.IsDefined(contentRequest.SourceType.Value) ||
                contentRequest.Detail.ContentType is null || !Enum.IsDefined(contentRequest.Detail.ContentType.Value))
            {

                throw new RequestValidationException("SourceType, ContentType are required and valid.");
            }

            switch (contentRequest.Detail.ContentType)
            {
                case ContentType.HOTEL
                     when string.IsNullOrWhiteSpace(contentRequest.Detail.ContentId):
                        throw new RequestValidationException("ContentId is required for HOTEL content.");

                case ContentType.CAR
                    when string.IsNullOrWhiteSpace(contentRequest.Detail.ContentId) || contentRequest.Detail.ContentId == "0":
                    {
                        if (string.IsNullOrWhiteSpace(contentRequest.Detail.SupplierName) || string.IsNullOrWhiteSpace(contentRequest.Detail.SippCode))
                            throw new RequestValidationException("SupplierName and SippCode are required when ContentId is not provided.");       
                        break;
                    }
            }
        }

    }
}
