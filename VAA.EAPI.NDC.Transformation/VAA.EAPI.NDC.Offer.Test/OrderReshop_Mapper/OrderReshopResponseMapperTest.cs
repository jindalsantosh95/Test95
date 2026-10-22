using System.Globalization;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop_Mapper
{
    public class OrderReshopResponseMapperTest
    {
        [Fact]
        public void MapReshopResponse_BaggageAssociationsMappings_WithOfferResponse()
        {
            var genericDataMapper = new GenericDataMapper();
            var offerResponseRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderReshop\\OfferResponse");
            var svcList = TestDataProvider.GetTestData<IEnumerable<OrderItemServices>>("NDCOffer\\OrderReshop\\ServiceList");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\orderDoc");

            var orderReshopRS = OrderReshopResponseMapper.MapReshopResponse(offerResponseRS, genericDataMapper, null, svcList, orderDoc);

            var orderesult = ((ReshopOffersType)((OrderReshopResponseType[])orderReshopRS.Items)[0].ReshopResults.Item).Offer[0];

            Assert.NotNull(orderReshopRS);

            Assert.Equal("ADULT_1", orderesult.BaggageAssociations[0].PaxRefID[0]);
            Assert.Equal("BAGA1", orderesult.BaggageAssociations[0].BaggageAllowanceRefID[0]);

            Assert.Equal("ADULT_1", orderesult.BaggageAssociations[1].PaxRefID[0]);
            Assert.Equal("CAB1", orderesult.BaggageAssociations[1].BaggageAllowanceRefID[0]);
        }

        [Fact]
        public void MapReshopResponse_JourneyOverviewMappings_WithOfferResponse()
        {
            var genericDataMapper = new GenericDataMapper();
            var offerResponseRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderReshop\\OfferResponse");
            var svcList = TestDataProvider.GetTestData<IEnumerable<OrderItemServices>>("NDCOffer\\OrderReshop\\ServiceList");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\orderDoc");

            var orderReshopRS = OrderReshopResponseMapper.MapReshopResponse(offerResponseRS, genericDataMapper, null, svcList, orderDoc);

            var orderesult = ((ReshopOffersType)((OrderReshopResponseType[])orderReshopRS.Items)[0].ReshopResults.Item).Offer[0];

            Assert.NotNull(orderReshopRS);

            Assert.Equal("JOUR1", orderesult.JourneyOverview.JourneyPriceClass[0].PaxJourneyRefID);
            Assert.Equal("PC1", orderesult.JourneyOverview.JourneyPriceClass[0].PriceClassRefID);
        }

        [Fact]
        public void MapReshopResponse_PaymentTimeLimitMappings_WithOfferResponse()
        {
            var genericDataMapper = new GenericDataMapper();
            var offerResponseRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderReshop\\offerResponse_HoldBooking");
            var svcList = TestDataProvider.GetTestData<IEnumerable<OrderItemServices>>("NDCOffer\\OrderReshop\\ServiceList");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\orderDocument_HoldBooking");

            var orderReshopRS = OrderReshopResponseMapper.MapReshopResponse(offerResponseRS, genericDataMapper, null, svcList, orderDoc);
            var orderesult = ((ReshopOffersType)((OrderReshopResponseType[])orderReshopRS.Items)[0].ReshopResults.Item).Offer[0];
            var paymentItem = orderesult.AddedOfferItem.First().PaymentTimeLimit.Item as PaymentTimeLimitDateType;

            Assert.NotNull(orderReshopRS);
            Assert.NotNull(paymentItem);
            Assert.Equal( DateTime.ParseExact("15-06-2026 11:47:00", "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture), paymentItem.PaymentTimeLimitDateTime);
        }
        [Fact]
        public void MapReshopResponse_ShouldReturnZeroNewAndDifferentialPrices_ForDisruptedHoldBooking()
        {
            var genericDataMapper = new GenericDataMapper();
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderReshop\\offerResponse_HoldBooking");
            var serviceList = TestDataProvider.GetTestData<IEnumerable<OrderItemServices>>("NDCOffer\\OrderReshop\\ServiceList");
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderReshop\\orderDocument_HoldBooking");

            var response = OrderReshopResponseMapper.MapReshopResponse(
                offerResponse, genericDataMapper, null, serviceList, orderDocument, isReshopDisruption: true);

            var offer = ((ReshopOffersType)((OrderReshopResponseType[])response.Items)[0].ReshopResults.Item).Offer[0];
            var priceDifferential = offer.AddedOfferItem.First().ReshopPrice.Item as PriceDifferentialType;

            Assert.NotNull(priceDifferential);
            Assert.Equal(DifferentialTypeCodeType.EvenExchange, priceDifferential.DifferentialTypeCode);
            Assert.NotNull(priceDifferential.DiffPrice);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.TotalAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueToAirlineAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueByAirlineAmount.Value);
            Assert.NotNull(priceDifferential.NewPrice);
            Assert.NotNull(priceDifferential.NewPrice.Price);
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.TotalAmount.Value);
            Assert.All(priceDifferential.NewPrice.Price.TaxSummary, taxSummary =>
            {
                Assert.Equal(0.00m, taxSummary.TotalTaxAmount.Value);
                Assert.Equal(0.00m, taxSummary.TotalRefundableTaxAmount.Value);
            });
            Assert.NotNull(priceDifferential.OldPrice);
            Assert.NotNull(priceDifferential.OldPrice.Price);
        }

        [Fact]
        public void MapReshopResponse_ShouldReturnAddCollect_ForNonDisruptedHoldBooking()
        {
            var genericDataMapper = new GenericDataMapper();
            var offerResponse = TestDataProvider.GetTestData<OfferResponse>(  "NDCOffer\\OrderReshop\\offerResponse_HoldBooking");
            var serviceList = TestDataProvider.GetTestData<IEnumerable<OrderItemServices>>( "NDCOffer\\OrderReshop\\ServiceList");
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>( "NDCOffer\\OrderReshop\\orderDocument_HoldBooking");

            var response = OrderReshopResponseMapper.MapReshopResponse(offerResponse, genericDataMapper, null, serviceList, orderDocument);
            var offer = ((ReshopOffersType)((OrderReshopResponseType[])response.Items)[0].ReshopResults.Item).Offer[0];
            var priceDifferential = offer.AddedOfferItem.First().ReshopPrice.Item as PriceDifferentialType;

            Assert.NotNull(priceDifferential);
            Assert.Equal( DifferentialTypeCodeType.AddCol, priceDifferential.DifferentialTypeCode);
        }

    }
}
