using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Mapper
{
    public class BrandMSResponseMapperTests
    {
        [Fact]
        public void Map_ShouldReturnMappedResponse()
        {
           //Arrange
           var brandRS = TestDataProvider.GetTestData<IEnumerable<BrandRS>>("NDCOrder\\BrandRS_AllPaxReturnJourney");
           var offerDetails = TestDataProvider.GetTestData<OfferDetails>("NDCOrder\\OfferDetails_AllPaxReturnJourney");
           var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OfferDoc_AllPaxReturnJourney");
            int penaltyIndex = 1;
            int baggageCounter = 1;
            int serviceCounter = 1;


            // Act
            var result = BrandMSResponseMapper.UpdateOfferDoc(brandRS , offerDetails , offerDoc , ref penaltyIndex , ref baggageCounter , ref serviceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DataList.BaggageAllowances, x => x.BaggageAllowanceTypeCode.Is("Checked"));
            Assert.Equal(2, result.DataList.BaggageAllowances.Where(x => x.BaggageAllowanceTypeCode.Is("CarryOn")).Count());

            Assert.Equal("23", result.DataList.BaggageAllowances.First(x => x.BaggageAllowanceTypeCode.Is("Checked")).FreeBaggageAllowance.WeightAllowance.First(x => x.WeightUnitOfMeasurement == EAPI.Domain.Virgin.Models.Products.Baggage.WeightUnitCodeType.KGM).MaximumWeightMeasure);
            Assert.Contains(result.DataList.BaggageAllowances, x => x.BaggageAllowanceTypeCode.Is("CarryOn") && x.FreeBaggageAllowance.WeightAllowance.Any(w => w.MaximumWeightMeasure == "10"));
            Assert.Contains(result.DataList.BaggageAllowances, x => x.BaggageAllowanceTypeCode.Is("CarryOn") && x.FreeBaggageAllowance.WeightAllowance.Any(w => w.MaximumWeightMeasure == "6"));

            Assert.Single(result.DataList.Penalty, x => x.Type.Is("Cancel"));
            Assert.Equal(2, result.DataList.Penalty.Where(x => x.Type.Is("Change")).Count());
            Assert.Contains(result.DataList.Penalty, p => p.Type.Is("Change") && p.MaxPrice.Amount.MonetaryAmount == "150.0");
            Assert.Contains(result.DataList.Penalty, p => p.Type.Is("Change") && p.MaxPrice.Amount.MonetaryAmount == "150.0");
            Assert.Contains(result.DataList.Penalty, p => p.Type.Is("Cancel") && p.MaxPrice.Amount.MonetaryAmount == "280.0");

            Assert.Equal(8, result.DataList.ProductComponents.Count());

            var adultOrderItem = result.Offers.First().OfferItems.First(x => x.Services.First().ServiceBeneficiaries.Contains("ADULT_1"));
            Assert.True(adultOrderItem.Services.SelectMany(x => x.BaggageRefIds).Distinct().SequenceEqual(["BAG1", "BAG2"]));
            Assert.True(adultOrderItem.Services.All(x => x.PenaltyRef.CancellationPenaltyRefIds.SequenceEqual(["PEN3"])));
            Assert.True(adultOrderItem.Services.All(x => x.PenaltyRef.ChangePenaltyRefIds.SequenceEqual(["PEN1", "PEN2"])));

            var youngOrderItem = result.Offers.First().OfferItems.First(x => x.Services.First().ServiceBeneficiaries.Contains("YOUNG_1"));
            Assert.True(youngOrderItem.Services.SelectMany(x => x.BaggageRefIds).Distinct().SequenceEqual(["BAG1", "BAG2"]));
            Assert.True(youngOrderItem.Services.All(x => x.PenaltyRef.CancellationPenaltyRefIds.SequenceEqual(["PEN3"])));
            Assert.True(youngOrderItem.Services.All(x => x.PenaltyRef.ChangePenaltyRefIds.SequenceEqual(["PEN1", "PEN2"])));

            var childOrderItem = result.Offers.First().OfferItems.First(x => x.Services.First().ServiceBeneficiaries.Contains("CHILD_1"));
            Assert.True(childOrderItem.Services.SelectMany(x => x.BaggageRefIds).Distinct().SequenceEqual(["BAG1", "BAG2"]));
            Assert.True(childOrderItem.Services.All(x => x.PenaltyRef.CancellationPenaltyRefIds.SequenceEqual(["PEN3"])));
            Assert.True(childOrderItem.Services.All(x => x.PenaltyRef.ChangePenaltyRefIds.SequenceEqual(["PEN1", "PEN2"])));

            var infantOrderItem = result.Offers.First().OfferItems.First(x => x.Services.First().ServiceBeneficiaries.Contains("INFANT_1"));
            Assert.True(infantOrderItem.Services.SelectMany(x => x.BaggageRefIds).Distinct().SequenceEqual(["BAG1", "BAG3"]));
            Assert.True(infantOrderItem.Services.All(x => x.PenaltyRef.CancellationPenaltyRefIds.SequenceEqual(["PEN3"])));
            Assert.True(infantOrderItem.Services.All(x => x.PenaltyRef.ChangePenaltyRefIds.SequenceEqual(["PEN1", "PEN2"])));

        }
    }
}
