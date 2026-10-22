using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class InsuranceQuoteQuery
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IOfferServiceProvider _offerServiceProvider;
        private readonly IRefServiceProvider _referenceServiceProvider;
        private readonly IInsuranceServiceProvider _insuranceServiceProvider;
        private readonly ILogger<InsuranceQuoteQuery> _logger;

        public InsuranceQuoteQuery(ILogger<InsuranceQuoteQuery> logger, IBasketServiceProvider basketServiceProvider, IOfferServiceProvider offerServiceProvider, IRefServiceProvider referenceServiceProvider, IInsuranceServiceProvider insuranceServiceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerServiceProvider = offerServiceProvider ?? throw new ArgumentNullException(nameof(offerServiceProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _insuranceServiceProvider = insuranceServiceProvider ?? throw new ArgumentNullException(nameof(insuranceServiceProvider));
        }

        [GraphQLName("InsuranceQuote")]
        [GraphQLDescription("Returns Insurance Quote for a given Trip stored under selected basketId and FE chosen quoteVersion")]
        public async Task<IEnumerable<QuoteResponse>> GetInsuranceQuoteAsync(string basketId, int quoteVersionId = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId))
                {
                    throw new ArgumentNullException(nameof(basketId), "Basket ID must be provided.");
                }

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);

                if (!basket.InsuranceQuotes.IsNullOrEmpty())
                {
                    return basket.InsuranceQuotes;
                }

                var offerId = basket.OfferIds.First();

                if (string.IsNullOrWhiteSpace(offerId))
                    throw new ArgumentException($"'{nameof(offerId)}'  must be spefied.");

                IEnumerable<AirportRefData> refAirportData = Enumerable.Empty<AirportRefData>();
                IEnumerable<CabinRefData> refCabinData = Enumerable.Empty<CabinRefData>();
                OfferResponse offer = null;

                Parallel.Invoke(
                        new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                        new Action(() => refCabinData = _referenceServiceProvider.RetrieveCabinDataAsync().GetAwaiter().GetResult()),
                        new Action(() => offer = _offerServiceProvider.OfferRetrieveAsync(basket.OfferIds.First()).GetAwaiter().GetResult())
                      );

                if (offer != null && offer.Pos.CountryCode != "GB" && offer.Pos.CountryCode != "US")
                    throw new InvalidOperationException(Constants.Application.NoQuoteAvailableErrorCode);

                var quoteRequest = InsuranceQuoteMapper.InsuranceQuoteRequestMapper(offer, basket, refAirportData, refCabinData, quoteVersionId);

                var quoteResponse = await _insuranceServiceProvider.InsuranceQuote(quoteRequest);

                var insuranceQuoteResponse = InsuranceQuoteMapper.InsuranceQuoteResponseMapper(quoteResponse, offer.Pos.CountryCode);
               
                if (insuranceQuoteResponse?.FirstOrDefault()?.Status?.Code.ToLower() == "success")
                    await _basketServiceProvider.SaveInsuranceQuoteAsync(basketId, insuranceQuoteResponse);

                return insuranceQuoteResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw;
            }
        }
    }
}