using Moq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Mapper.OrderQuote;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote_ResponseMapper
{
    public class OrderQuoteRSMappingTests
    {
        private static Mock<IGenericDataMapper> genericDataMapper = new Mock<IGenericDataMapper>();

        [Fact]
        public void Should_Return_ValidResponse_When_OfferResponse_IsValid()
        {
            var request = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\offerDocument");

            var result = OrderQuoteRSMapper.OrderQuoteRSMapping( false,false ,request, offerDoc, genericDataMapper.Object, null, true, false, null, false);
            var orderItem = (OrderReshopResponseType)result.Value.Items[0];

            Assert.NotNull(request.Offers);
            Assert.NotNull(result);
            Assert.Equal("VS932DE6DIF24", orderItem.Order.OrderID);
            Assert.Equal("ADULT_1", orderItem.DataLists.PaxList[0].PaxID);
            Assert.Equal("ADT", orderItem.DataLists.PaxList[0].PTC);
            Assert.Equal("OD1", orderItem.DataLists.OriginDestList[0].OriginDestID);
            Assert.Equal("LHR", orderItem.DataLists.OriginDestList[0].OriginCode);
            Assert.Equal("BOS", orderItem.DataLists.OriginDestList[0].DestCode);
            Assert.NotNull(orderItem.ReshopResults);
            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
            Assert.Null(((ReshopOffersType)orderItem.ReshopResults.Item).RequotedOffer.TotalPrice);
            Assert.Equal("80d93755-ef51-4002-a161-97f7c7bf111a|clBO2aBb2N1MPvjTNWQsIL00E-1", ((ReshopOffersType)orderItem.ReshopResults.Item).RequotedOffer.AddedOfferItem.First().OfferItemID);
            Assert.Null(orderItem.Warning);
            Assert.NotNull(orderItem.PaymentFunctions);
            Assert.Equal("CA", orderItem.PaymentFunctions[0].PaymentSupportedMethod.PaymentTypeCode);
        }

        [Fact]
        public void Should_Return_PaymentFunctions_For_EasyPay()
        {
            var request = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\offerDocument");
            offerDoc.FormOfPayments = PaymentDetails.FormOfPayment.EasyPay;

            var result = OrderQuoteRSMapper.OrderQuoteRSMapping(false, false ,request, offerDoc, genericDataMapper.Object, null, false, true, null, false);
            var orderItem = (OrderReshopResponseType)result.Value.Items[0];

            Assert.NotNull(result);
            Assert.NotNull(orderItem.PaymentFunctions);
            Assert.Equal("AC", orderItem.PaymentFunctions[0].PaymentSupportedMethod.PaymentTypeCode);
        }

        [Fact]
        public void Should_Return_PaymentFunctions_For_Card()
        {
            var request = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\offerDocument");
            offerDoc.FormOfPayments = PaymentDetails.FormOfPayment.CardEnum;

            var bspMarketPaymentData = new BspMarketPaymentData()
            {
                IsCreditCardAccepted = true,
                Visa = true
            };

            var result = OrderQuoteRSMapper.OrderQuoteRSMapping(false, false, request, offerDoc, genericDataMapper.Object, null, false, true, bspMarketPaymentData, false);
            var orderItem = (OrderReshopResponseType)result.Value.Items[0];

            Assert.NotNull(result);
            Assert.NotNull(orderItem.PaymentFunctions);
            Assert.Equal("CC", orderItem.PaymentFunctions[0].PaymentSupportedMethod.PaymentTypeCode);
        }
        [Fact]
        public void Should_Map_TicketedDisruption_AsEvenExchange_WithZeroNewAndDifferentialPrices()
        {
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\offerDocument");

            var result = OrderQuoteRSMapper.OrderQuoteRSMapping( isHoldBookingReshop: false, isReshopDisruption: true, offerResponse: offerResponse, offerDocument: offerDoc, genericDataMapper: genericDataMapper.Object, nDCWarnings: null, isCashPaymentAllowed: true, isEasyFopAllowed: false, bspMarketPaymentData: null,isNonIataAgent: false);

            var orderItem = (OrderReshopResponseType)result.Value.Items[0];
            var reshopOffers = (ReshopOffersType)orderItem.ReshopResults.Item;

            Assert.NotNull(reshopOffers.Offer);
            Assert.Empty(reshopOffers.Offer);

            var requotedOffer = reshopOffers.RequotedOffer;
            var addedOfferItem = requotedOffer.AddedOfferItem.First();
            var priceDiff = addedOfferItem.ReshopPrice.Item as PriceDifferentialType;

            Assert.NotNull(priceDiff);
            Assert.Equal(DifferentialTypeCodeType.EvenExchange, priceDiff.DifferentialTypeCode);
            Assert.Equal(0.00m, priceDiff.NewPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDiff.NewPrice.Price.TotalAmount.Value);
            Assert.Equal(0.00m, priceDiff.DiffPrice.Price.DueToAirlineAmount.Value);
            Assert.Equal(0.00m, priceDiff.DiffPrice.Price.DueByAirlineAmount.Value);
            Assert.All(priceDiff.NewPrice.Price.TaxSummary, taxSummary =>
            {
                Assert.Equal(0.00m, taxSummary.TotalTaxAmount.Value);
                Assert.NotNull(taxSummary.TotalRefundableTaxAmount);
                Assert.Equal(0.00m, taxSummary.TotalRefundableTaxAmount.Value);
                Assert.All(taxSummary.Tax, tax => Assert.Equal(0.00m, tax.Amount.Value));
            });
            Assert.NotEqual(0.00m, priceDiff.OldPrice.Price.TotalAmount.Value);

            Assert.Null(orderItem.DataLists.PenaltyList);
        }

        [Fact]
        public void Should_Map_DisruptedHoldBooking_AsEvenExchange_WithZeroNewAndDifferentialPrices()
        {
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\offerDocument");

            var result = OrderQuoteRSMapper.OrderQuoteRSMapping( isHoldBookingReshop: true,isReshopDisruption: true, offerResponse: offerResponse, offerDocument: offerDoc,genericDataMapper: genericDataMapper.Object, nDCWarnings: null, isCashPaymentAllowed: true,isEasyFopAllowed: false, bspMarketPaymentData: null,isNonIataAgent: false);

            var orderItem = Assert.IsType<OrderReshopResponseType>(result.Value.Items[0]);
            var reshopOffers = Assert.IsType<ReshopOffersType>(orderItem.ReshopResults.Item);
            var requotedOffer = reshopOffers.RequotedOffer;
            var addedOfferItem = Assert.Single(requotedOffer.AddedOfferItem);
            var priceDifferential = (PriceDifferentialType)addedOfferItem.ReshopPrice.Item;


            Assert.NotNull(reshopOffers.RequotedOffer);
            Assert.Equal(DifferentialTypeCodeType.EvenExchange, priceDifferential.DifferentialTypeCode);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.TotalAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueToAirlineAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueByAirlineAmount.Value);
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.TotalAmount.Value);
            Assert.All(priceDifferential.DiffPrice.Price.TaxSummary, taxSummary =>
            {
                Assert.Equal(0.00m, taxSummary.TotalTaxAmount.Value);
                Assert.All(taxSummary.Tax, tax => Assert.Equal(0.00m, tax.Amount.Value));
            });
            
            Assert.NotEqual(0.00m, priceDifferential.OldPrice.Price.TotalAmount.Value);
            Assert.NotEmpty(priceDifferential.OldPrice.Price.TaxSummary);
            Assert.NotNull(reshopOffers.Offer);
            Assert.Empty(reshopOffers.Offer);
        }
    }
}
