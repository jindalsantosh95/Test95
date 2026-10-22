using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Queries;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;


namespace VAA.EAPI.GraphQL.Domain.Book.Tests.Queries
{
    public class InsuranceQuoteQueryTests
    {
        private readonly Mock<ILogger<InsuranceQuoteQuery>> _loggerMock = new();
        private readonly Mock<IBasketServiceProvider> _basketServiceProviderMock = new();
        private readonly Mock<IOfferServiceProvider> _offerServiceProviderMock = new();
        private readonly Mock<IRefServiceProvider> _referenceServiceProviderMock = new();
        private readonly Mock<IInsuranceServiceProvider> _insuranceServiceProviderMock = new();

        public InsuranceQuoteQueryTests()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        private InsuranceQuoteQuery CreateQuery()
        {
            return new InsuranceQuoteQuery(
                _loggerMock.Object,
                _basketServiceProviderMock.Object,
                _offerServiceProviderMock.Object,
                _referenceServiceProviderMock.Object,
                _insuranceServiceProviderMock.Object
            );
        }

        [Fact]
        public async Task GetInsuranceQuoteAsync_ThrowsIfBasketIdNullOrEmpty()
        {
            var query = CreateQuery();

            await Assert.ThrowsAsync<ArgumentNullException>(() => query.GetInsuranceQuoteAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => query.GetInsuranceQuoteAsync(""));
        }

        [Fact]
        public async Task GetInsuranceQuoteAsync_ReturnsInsuranceQuotes_IfPresent()
        {
            var basketId = "basket1";
            var insuranceQuotes = new List<QuoteResponse> { new QuoteResponse() };
            _basketServiceProviderMock
                .Setup(x => x.RetrieveBasketAsync(basketId))
                .ReturnsAsync(new Models.Basket.Basket { InsuranceQuotes = insuranceQuotes });

            var query = CreateQuery();

            var result = await query.GetInsuranceQuoteAsync(basketId);

            Assert.Equal(insuranceQuotes, result);
        }

        [Fact]
        public async Task GetInsuranceQuoteAsync_ThrowsIfOfferIdIsNullOrWhiteSpace()
        {
            var basketId = "basket2";
            _basketServiceProviderMock
                .Setup(x => x.RetrieveBasketAsync(basketId))
                .ReturnsAsync(new Models.Basket.Basket { InsuranceQuotes = null, OfferIds = new List<string> { "" } });

            var query = CreateQuery();

            await Assert.ThrowsAsync<ArgumentException>(() => query.GetInsuranceQuoteAsync(basketId));
        }

        [Fact]
        public async Task GetInsuranceQuoteAsync_ThrowsIfCountryCodeNotGBorUS()
        {
            var basketId = "basket3";
            _basketServiceProviderMock
                .Setup(x => x.RetrieveBasketAsync(basketId))
                .ReturnsAsync(new Models.Basket.Basket { InsuranceQuotes = null, OfferIds = new List<string> { "offer1" } });

            _referenceServiceProviderMock
                .Setup(x => x.RetrieveAirportDataAsync())
                .ReturnsAsync(new List<AirportRefData>());
            _referenceServiceProviderMock
                .Setup(x => x.RetrieveCabinDataAsync())
                .ReturnsAsync(new List<CabinRefData>());
            _offerServiceProviderMock
                .Setup(x => x.OfferRetrieveAsync("offer1"))
                .ReturnsAsync(new OfferResponse { Pos = new Pos { CountryCode = "IN" } });

            var query = CreateQuery();

            await Assert.ThrowsAsync<InvalidOperationException>(() => query.GetInsuranceQuoteAsync(basketId));
        }

        [Fact]
        public async Task GetInsuranceQuoteAsync_SavesInsuranceQuote_WhenStatusIsSuccess_UK()
        {
            string basketId = "dacef25c-3d54-4e4f-aa98-c2214facea89";
            string offerId = "a8203005-601c-48bd-8418-5f00c14026bc|e13f199a-3f7b-4e0a-9a1f-79ceb5d2277d-FL-1";
            var insuranceQuoteResponse = TestDataProvider.GetTestData<InsuranceQuoteResponse>("Book\\InsuranceQuoteResponseUK");
            var basket = TestDataProvider.GetTestData<Models.Basket.Basket>("Book\\RetriveBasketResponseUK");
            var newBasket = TestDataProvider.GetTestData<Models.Basket.Basket>("Book\\RetriveBasketResponseUK");
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("Book\\OfferResponseUK");

            _basketServiceProviderMock
                .Setup(x => x.RetrieveBasketAsync(basketId))
                .ReturnsAsync(basket);

            _referenceServiceProviderMock
                .Setup(x => x.RetrieveAirportDataAsync())
                .ReturnsAsync(new List<AirportRefData>() { new() { AirportCode = "LHR", CountryCode = "GB" }, new() { AirportCode = "DEL", CountryCode = "IN" } });
            _referenceServiceProviderMock
                .Setup(x => x.RetrieveCabinDataAsync())
                .ReturnsAsync(new List<CabinRefData>());
            _offerServiceProviderMock
                .Setup(x => x.OfferRetrieveAsync(offerId))
                .ReturnsAsync(offerResponse);

            var insuranceQuoteResponseMapped = InsuranceQuoteMapper.InsuranceQuoteResponseMapper(insuranceQuoteResponse, "GB");
          
            _insuranceServiceProviderMock
                .Setup(x => x.InsuranceQuote(It.IsAny<InsuranceQuoteRequest>()))
                .ReturnsAsync(insuranceQuoteResponse);

            newBasket.InsuranceQuotes = insuranceQuoteResponseMapped;
            _basketServiceProviderMock
                .Setup(x => x.SaveInsuranceQuoteAsync(basketId, insuranceQuoteResponseMapped))
                .ReturnsAsync(newBasket);

            var query = CreateQuery();

            var result = await query.GetInsuranceQuoteAsync(basketId);

            Assert.Equal(insuranceQuoteResponseMapped.FirstOrDefault()?.Status.Code, result.FirstOrDefault()?.Status.Code);

            _insuranceServiceProviderMock.Verify(x => x.InsuranceQuote(It.IsAny<InsuranceQuoteRequest>()), Times.Once);
        }


        [Fact]
        public async Task GetInsuranceQuoteAsync_SavesInsuranceQuote_WhenStatusIsSuccess_US()
        {
            string basketId = "2d70c4bb-5147-4004-9430-0a8b6d8683db";
            string offerId = "2e9fa0e0-f900-47ce-a4c9-535d4b523250|cdc1aa49-8324-4d63-9bf9-780dbbf1b375-FL-1";
            var insuranceQuoteResponse = TestDataProvider.GetTestData<InsuranceQuoteResponse>("Book\\InsuranceQuoteResponseUS");
            var basket = TestDataProvider.GetTestData<Models.Basket.Basket>("Book\\RetriveBasketResponseUS");
            var newBasket = TestDataProvider.GetTestData<Models.Basket.Basket>("Book\\RetriveBasketResponseUS");
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("Book\\OfferResponseUS");

            _basketServiceProviderMock.Setup(x => x.RetrieveBasketAsync(basketId))
                                      .ReturnsAsync(basket);

            _referenceServiceProviderMock.Setup(x => x.RetrieveAirportDataAsync())
                                         .ReturnsAsync([new() { AirportCode = "JFK", CountryCode = "US" }, new() { AirportCode = "DEL", CountryCode = "IN" }]);

            _referenceServiceProviderMock.Setup(x => x.RetrieveCabinDataAsync())
                                         .ReturnsAsync([]);

            _offerServiceProviderMock.Setup(x => x.OfferRetrieveAsync(offerId))
                                     .ReturnsAsync(offerResponse);

            var insuranceQuoteResponseMapped = InsuranceQuoteMapper.InsuranceQuoteResponseMapper(insuranceQuoteResponse, "US");

            _insuranceServiceProviderMock.Setup(x => x.InsuranceQuote(It.IsAny<InsuranceQuoteRequest>()))
                                         .ReturnsAsync(insuranceQuoteResponse);

            newBasket.InsuranceQuotes = insuranceQuoteResponseMapped;

            _basketServiceProviderMock.Setup(x => x.SaveInsuranceQuoteAsync(basketId, insuranceQuoteResponseMapped))
                                      .ReturnsAsync(newBasket);

            var query = CreateQuery();

            var result = await query.GetInsuranceQuoteAsync(basketId);

            Assert.Equal(insuranceQuoteResponseMapped.FirstOrDefault()?.Status.Code, result.FirstOrDefault()?.Status.Code);

            _insuranceServiceProviderMock.Verify(x => x.InsuranceQuote(It.IsAny<InsuranceQuoteRequest>()), Times.Once);

        }
    }

}

