using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.OrderQuote;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote_ResponseMapper
{
    public class OrderQuoteHoldBookingRSMappingTests
    {
        [Fact]
        public void Should_Return_ValidResponse_When_OfferResponse_IsValid()
        {
            var bspMarketPaymentData = new BspMarketPaymentData();
            var OfferResponse = new OfferResponse() { OrderId = "VS932FFXAWX24", NoChangeId = true };
            var result = OrderQuoteRSMapper.OrderQuoteHoldBookingRSMapping(OfferResponse, false, false, bspMarketPaymentData, false);

            var orderItem = (OrderReshopResponseType)result.Value.Items[0];

            Assert.NotNull(result);
            Assert.Equal("VS932FFXAWX24", orderItem.Order.OrderID);
            Assert.True(((NoChangeIndType)orderItem.ReshopResults.Item).NoChangeInd);
            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
        }

        [Fact]
        public void Should_Return_ValidResponse_When_OfferResponse_OwnerCodeValid()
        {
            var bspMarketPaymentData = new BspMarketPaymentData();
            var OfferResponse = new OfferResponse() { OrderId = "VS932FFXAWX24", NoChangeId = true };
            var result = OrderQuoteRSMapper.OrderQuoteHoldBookingRSMapping(OfferResponse, false, false, bspMarketPaymentData, false);

            var orderItem = (OrderReshopResponseType)result.Value.Items[0];

            Assert.NotNull(result);
            Assert.Equal(Constants.Application.OwnerCode, orderItem.Order.OwnerCode);
            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
        }

        [Fact]
        public void Should_AddCard_When_CreditCardAccepted()
        {
            var offerResponse = new OfferResponse
            {
                OrderId = "VS932FFXAWX24",
                NoChangeId = true
            };

            var bspMarketPaymentData = new BspMarketPaymentData 
            { 
                IsCreditCardAccepted = true ,
                Visa = true
            };

            var result = OrderQuoteRSMapper.OrderQuoteHoldBookingRSMapping(offerResponse,true,true,bspMarketPaymentData,false);
            var orderItem = (OrderReshopResponseType) result.Value.Items[0];

            Assert.NotNull(result);
            Assert.NotNull(orderItem.PaymentFunctions);
            Assert.Equal("CA", orderItem.PaymentFunctions[0].PaymentSupportedMethod.PaymentTypeCode);
            Assert.Equal("AC", orderItem.PaymentFunctions[1].PaymentSupportedMethod.PaymentTypeCode);
            Assert.Equal("CC", orderItem.PaymentFunctions[2].PaymentSupportedMethod.PaymentTypeCode);
        }
    }
}
