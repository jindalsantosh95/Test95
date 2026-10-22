using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Sustain;


namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CarbonIndexQuery
    {
        private readonly ILogger<CarbonIndexQuery> _logger;
        private readonly ISustainService _sustainService;
        private readonly IOfferRetrieveService _offerRetrieveService;


        public CarbonIndexQuery(ILogger<CarbonIndexQuery> logger, IOfferRetrieveService offerRetrieveService, ISustainService sustainService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerRetrieveService = offerRetrieveService ?? throw new ArgumentNullException(nameof(offerRetrieveService));
            _sustainService = sustainService ?? throw new ArgumentNullException(nameof(sustainService));
        }

        [GraphQLName("CarbonIndex")]
        [GraphQLDescription("Retrieves CO2e info from Suatainability MS")]
        public async Task<IEnumerable<CarbonIndexResponse>> GetCarbonIndex(CarbonIndexRequest request)
        {
            IEnumerable<CarbonIndexResponse> carbonIndexResults = Enumerable.Empty<CarbonIndexResponse>();

            if (!Constants.Environment.EnableCarbonIndexIntegration)
            {
                return carbonIndexResults;
            }

            try
            {
                IEnumerable<string> passengerTypeCodes = PassengerTypeList.ParseString(request.PassengerCounts);
                var passengerCountWithoutInfant = passengerTypeCodes.Count(x => !x.Contains(Common.Constants.Application.DeltaPtcInfant));

                var offerResponse = await _offerRetrieveService.OfferRetrieveAsync(new Models.FlightOffer.OfferSearchRequest() { OfferID = request.OfferId });

                var sustainMSRequests = OfferToSustainMapper.Map(offerResponse);

                var sustainMSResponses = await _sustainService.RetrieveSustainResponseCollectionAsync(sustainMSRequests);

                carbonIndexResults = SustainToGraphqlMapper.MaptoGraphqlResponse(sustainMSResponses, passengerCountWithoutInfant, offerResponse.IsAwardSearchResult);

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Correlation Id - {AsyncContext.Current.CorrelationId},CarbonIndex query request \n {Json.Serialize(request)}");
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
            }
            return carbonIndexResults;
        }


    }
}