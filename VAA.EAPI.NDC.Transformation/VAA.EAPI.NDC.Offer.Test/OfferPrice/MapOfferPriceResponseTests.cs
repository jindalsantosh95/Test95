using Microsoft.Extensions.Azure;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class MapOfferPriceResponseTests
    {
        private readonly IEnumerable<NDCWarning> warningList;

        public MapOfferPriceResponseTests()
        {
            warningList = Enumerable.Empty<NDCWarning>();
        }

        [Fact]
        public void Should_ReturnPaymentOption_NULL_When_NonIATAAgent()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = false;
            bool isPaymentallowed = false;
            bool isEasyFOPallowed = false;
            bool isCardFOPallowed = false;
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(offerDoc, genericDataMapper, false, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Null(paymentoption);

        }

        [Fact]
        public void Should_ReturnPaymentOption_CreditCard_When_IATAAgent()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = false;
            bool isPaymentallowed = true;
            bool isEasyFOPallowed = true;
            bool isCardFOPallowed = true;

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            bspMarketData.IsCreditCardAccepted = true;

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(offerDoc, genericDataMapper, true, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Equal(paymentoption, "CA");

        }

        [Fact]
        public void Should_ReturnSuccesResult_withMultisegment()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = false;
            bool isPaymentallowed = true;
            bool isEasyFOPallowed = true;
            bool isCardFOPallowed = true;

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponse");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            bspMarketData.IsCreditCardAccepted = true;

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(offerDoc, genericDataMapper, true, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;
            var pricedOffer = offerDoc.Offers.FirstOrDefault(x => !x.UpsellOffer);

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Equal("CA", paymentoption);

            Assert.NotNull(pricedOffer);
            Assert.Equal("1027.36", pricedOffer.SalePrice?.First(s => s.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount);

        }

        [Fact]
        public void GetParties_ShouldReturnSuccess()
        {

            var genericDataMapper = new GenericDataMapper();
            var airShoppingRQ = TestDataProvider.GetXmlTestData<Domain.RequestResponseModels.AirShoppingRQ.IATA_AirShoppingRQ>("NDCOffer\\AirShopping\\AirShoppingRQ");
            var parties = OfferRequestMapper.GetParties(airShoppingRQ);

            Assert.NotNull(parties);
            Assert.Equal(1, parties.Participants.Count());
            Assert.Equal(parties.Participants.First().AgencyID, "91269021");
            Assert.Equal(parties.Participants.First().Name, "go4travel");
        }

        [Fact]
        public void Should_ReturnPaymentOption_NULL_When_NonIATAAgentForMultiPassenger()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = false;
            bool isPaymentallowed = false;
            bool isEasyFOPallowed = false;
            bool isCardFOPallowed = false;

            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponsewith2ADT1CHD1GBE1INF");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(OfferDoc, genericDataMapper, false, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Null(paymentoption);

        }

        [Fact]
        public void Should_ReturnPaymentOption_CreditCard_When_IATAAgentForMultiPassenger()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = true;
            bool isPaymentallowed = true;
            bool isEasyFOPallowed = false;
            bool isCardFOPallowed = false;

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponsewith2ADT1CHD1GBE1INF");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            bspMarketData.IsCreditCardAccepted = true;

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(offerDoc, genericDataMapper, true, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Equal("694.00", (((OfferPriceResponseType[])iATAOfferPriceRS.Items)[0]).PricedOffer.OfferItem.FirstOrDefault()?.Price.BaseAmount.Value.ToString());
            Assert.Equal("CA", paymentoption);
            Assert.Equal("SV1", (((OfferPriceResponseType[])iATAOfferPriceRS.Items)[0]).PricedOffer.OfferItem.FirstOrDefault()?.Service.FirstOrDefault()?.ServiceID.ToString());
            Assert.Equal("SV13", (((OfferPriceResponseType[])iATAOfferPriceRS.Items)[0]).OtherOffers.Offer.FirstOrDefault()?.OfferItem.FirstOrDefault()?.Service.FirstOrDefault()?.ServiceID.ToString());
        }

        [Fact]
        public void Should_ReturnSuccesResult_withMultisegmentForMultiPassenger()
        {
            var genericDataMapper = new GenericDataMapper();
            bool? itaAccountCode = false;
            bool isPaymentallowed = true;
            bool isEasyFOPallowed = true;
            bool isCardFOPallowed = true;

            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OfferPrice\\OfferResponsewith2ADT1CHD1GBE1INF");

            var bspMarketData = TestDataProvider.GetTestData<BspMarketPaymentData>("NDCOffer\\OfferPrice\\BspMarketDataResponse");

            bspMarketData.IsCreditCardAccepted = true;

            var iATAOfferPriceRS = OfferPriceRSMapper.MapOfferPriceResponse(offerDoc, genericDataMapper, true, bspMarketData, warningList, itaAccountCode, isPaymentallowed, isEasyFOPallowed, isCardFOPallowed);

            var paymentoption = iATAOfferPriceRS.Items.Select(it => (OfferPriceResponseType)it)?.FirstOrDefault().PaymentFunctions?.FirstOrDefault().PaymentSupportedMethod?.PaymentTypeCode;

            Assert.True(iATAOfferPriceRS.Items.Any());
            Assert.Null(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(ErrorType))));
            Assert.NotNull(iATAOfferPriceRS.Items.FirstOrDefault(it => it.GetType().Equals(typeof(OfferPriceResponseType))));
            Assert.Equal(paymentoption, "CA");

        }

    }
}
