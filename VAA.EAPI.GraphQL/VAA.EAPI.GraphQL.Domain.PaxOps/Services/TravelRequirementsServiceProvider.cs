using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class TravelRequirementsServiceProvider : ITravelRequirementsServiceProvider
    {
        private const string CountryNamePlaceholder = "{countryName}";
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CheckInMsOcpKey) } };

        protected readonly IHttpConnector _connector;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<TravelRequirementsServiceProvider> _logger;

        public TravelRequirementsServiceProvider(
            IHttpConnector connector,
            IReferenceServiceProvider referenceServiceProvider,
            ILogger<TravelRequirementsServiceProvider> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public TravelRequirementsServiceProvider(IHttpConnector connector)
            : this(connector, new ReferenceServiceProvider(connector), NullLogger<TravelRequirementsServiceProvider>.Instance)
        {
        }

        public async Task<TravelRequirementsServiceResponse> GetTravelRequirementsAsync(TravelRequirementsRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SavePassportAsync(Infrastructure.Models.TravelRequirements.SavePassportServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SavePassport";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SaveReturnDateAsync(TravelRequirementsSaveReturnDateServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveReturnDate";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SaveEmergencyContactAsync(SaveEmergencyContactServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveEmergencyContact";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SaveContactTracingAsync(TravelRequirementsSaveContactTracingServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveContactTracing";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SaveGovernmentFormsAttestationAsync(TravelRequirementsSaveGovernmentFormsAttestationServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveGovernmentFormsAttestation";

            return await SendAndEnrichAsync(uri, request);
        }

        public async Task<TravelRequirementsServiceResponse> SaveSecondaryDocumentAttestationAsync(TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest request)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveSecondaryDocumentAttestation";

            return await SendAndEnrichAsync(uri, request);
        }

        private async Task<TravelRequirementsServiceResponse> SendAndEnrichAsync<TRequest>(string uri, TRequest request)
        {
            var response = (await _connector.SendAsync<TravelRequirementsServiceResponse>(
                uri,
                _headers,
                string.Empty,
                HttpMethod.Post,
                Json.Serialize(request))).Response;

            var secondaryDocuments = response?.Passengers?
                .SelectMany(passenger => passenger.SliceRequirements ?? [])
                .SelectMany(requirement => requirement.SecondaryDocuments ?? [])
                .Where(document => !document.CountryCode.IsNullOrWhiteSpace())
                .ToArray() ?? [];

            if (secondaryDocuments.Length > 0)
            {
                var countries = await _referenceServiceProvider.GetCountryProfile() ?? [];
                foreach (var document in secondaryDocuments)
                {
                    var country = countries.FirstOrDefault(country =>
                        country.ISOCountryCode.Is(document.CountryCode) ||
                        country.IATACountryCode.Is(document.CountryCode));

                    document.CountryCode3Letter = country?.ISOCountryCode;
                    document.CountryCode = country?.IATACountryCode ?? document.CountryCode;
                    document.CountryName = country?.CountryName;

                    var countryName = document.CountryName.IsNullOrWhiteSpace()
                        ? document.CountryCode
                        : document.CountryName;
                    if (countryName.IsNullOrWhiteSpace())
                        continue;

                    document.DisplayName = document.DisplayName?.Replace(CountryNamePlaceholder, countryName);
                    foreach (var application in document.SecondaryDocumentApplicationOptions ?? [])
                        application.Message = application.Message.Replace(CountryNamePlaceholder, countryName);
                }
            }

            var destinations = response?.Slices?
                .SelectMany(slice => new[] { slice.Destination }
                    .Concat(slice.Segments?.Select(segment => segment.Destination) ?? []))
                .Where(destination => destination != null)
                .ToArray() ?? [];

            var destinationsWithAirportCodes = destinations
                .Where(destination => !destination.Code.IsNullOrWhiteSpace())
                .ToArray();

            foreach (var destination in destinations.Except(destinationsWithAirportCodes))
                destination.CityName = null;

            if (destinationsWithAirportCodes.Length == 0)
                return response;

            IEnumerable<Models.ReferenceData.AirportDetails> airports;
            try
            {
                airports = await _referenceServiceProvider.RetrieveAirportDataAsync() ?? [];
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Travel requirements: Failed to retrieve airport reference data.");
                airports = [];
            }

            foreach (var destination in destinationsWithAirportCodes)
            {
                destination.CityName = airports
                    .FirstOrDefault(airport => airport.AirportCode.Is(destination.Code))
                    ?.CityName;
            }

            return response;
        }
    }
}
