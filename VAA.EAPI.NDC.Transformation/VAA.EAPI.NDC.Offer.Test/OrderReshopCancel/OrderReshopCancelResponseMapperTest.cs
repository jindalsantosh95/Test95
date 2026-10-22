using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshopCancel
{
    public class OrderReshopCancelResponseMapperTest
    {
        [Fact]
        public void MapReshopCancelResponse_PriceDifferentialCode_WithOfferResponse()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderCancel\\OrderDoc");
            var offerMSresponse = TestDataProvider.GetTestData<CancelReshopResponse>("NDCOffer\\OrderCancel\\OfferMSResponse_RFC");

            var orderReshopCancelRS = OrderReshopCancelResponseMapper.MapReshopResponse(orderDoc, offerMSresponse);

            var orderResult = ((ReshopOffersType)((OrderReshopResponseType[])orderReshopCancelRS.Items)[0].ReshopResults.Item).Offer[0];

            Assert.NotNull(orderReshopCancelRS);
            Assert.Equal("Refund", orderResult.DeleteOrderItem.First().PriceDifferential.DifferentialTypeCode.ToString());
            Assert.Equal("VS", orderResult.OwnerCode);
            Assert.Equal("GBP", orderResult.DeleteOrderItem.First().PriceDifferential.NewPrice.Price.TaxSummary.First().TotalTaxAmount.CurCode);
            Assert.Equal("VS932CMR43724-1", orderResult.DeleteOrderItem.First().ExistingOrderItem.First().OrderItemRefID);
            Assert.Equal("0.00", orderResult.DeleteOrderItem.First().PriceDifferential.NewPrice.Price.TotalAmount.Value.ToString());
        }
    }
}
