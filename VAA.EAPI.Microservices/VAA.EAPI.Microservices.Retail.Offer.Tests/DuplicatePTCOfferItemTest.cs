using Microsoft.CodeAnalysis;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class DuplicatePTCOfferItemTest
    {
        private readonly string docID;
        private readonly Mock<ICachedPriceClass> _cachedPriceClass;
        private readonly Mock<ICachedCabin> _cachedCabin;
        private readonly Mock<ICachedCabinBaggage> _cachedCabinBaggage;
        private readonly Mock<ICachedPieceAllowance> _cachedPieceAllowance;
        private readonly Mock<ICachedAirportDetails> _cachedAirportDetails;
        public DuplicatePTCOfferItemTest()
        {
            docID = Guid.NewGuid().ToString();
            _cachedPriceClass = new Mock<ICachedPriceClass>();
            _cachedAirportDetails = new Mock<ICachedAirportDetails>();
            _cachedCabin = new Mock<ICachedCabin>();
            _cachedCabinBaggage = new Mock<ICachedCabinBaggage>();
            _cachedPieceAllowance = new Mock<ICachedPieceAllowance>();
            _cachedCabin.Setup(x => x.GetCachedCabinData(It.IsAny<string>())).ReturnsAsync(new CabinTableEntity()
            {
                CabinCode = "2",
                CabinName = "Bussiness"
            });
            _cachedPriceClass.Setup(x => x.GetCachedPriceClassData(It.IsAny<string>())).ReturnsAsync(new PriceClassTableEntity()
            {
                RowKey = "ZCF",
                ProductName = "Economy Flex"
            });
            _cachedCabinBaggage.Setup(X => X.GetCachedCabinBaggageInformation(It.IsAny<string>())).ReturnsAsync(new List<CabinBaggageEntity>()
            {
               new CabinBaggageEntity{
                   RowKey = "CBO1",
                   AllowanceType="Dimension",
                   DimensionCategory ="Heiht",
                   TotalQuantity = "1",
                   Value = "55",
                   UnitCode="CM"
               }
            });
            _cachedPieceAllowance.Setup(X => X.GetCachedPieceAllowanceInformation(It.IsAny<string>())).ReturnsAsync(new List<PieceAllowanceTableEntity>()
            {
               new PieceAllowanceTableEntity{
                   RowKey = "HeihtCM",
                   DimensionCategory ="Height",
                   MaxMeasure = "55",
                   UnitCode="CM"
               }
            });

            OfferTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void GetOfferItems_IgnoreDuplicatePTC()
        {
            int offerItemId = 1;
            string previousPTC = string.Empty;
            string currentPTC = string.Empty;
            var itaRequest = TestDataProvider.GetXmlTestData<search>("OfferMS\\ITAReshopRequest");
            var requestData = TestDataProvider.GetTestData<ReshopRequest>("OfferMS\\OrderReshopDuplicatePtcCheck");
            var result = TestDataProvider.GetXmlTestData<result>("ITAService\\Response\\Reshop_Response__With_Repeting_PTC");
            var orderReshopResponse = ReshopBundleResponseMapper.ReshopResponseMapper(itaRequest, result, requestData, _cachedPriceClass.Object, _cachedCabin.Object, docID, DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat), _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object).Result;
            var offerItems = GetAllOfferItems(orderReshopResponse);
            var bundledSolnSummary = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultBundledSolutionSummary)) as resultBundledSolutionSummary;
            foreach (var soln in bundledSolnSummary.solution)
            {
                foreach (var pricing in soln.pricing)
                {
                    currentPTC = pricing.passenger.FirstOrDefault().ptc.FirstOrDefault();
                    if (previousPTC.ToUpper().Equals(currentPTC.ToUpper()))
                    {
                        Assert.Null(offerItems.FirstOrDefault(x => x.OfferItemId.Contains($"{docID}{Constants.Application.Pipeline}{soln.id}-{offerItemId}")));
                    }
                    previousPTC = currentPTC;
                    offerItemId++;
                }
                previousPTC = string.Empty;
                currentPTC = string.Empty;
                offerItemId = 1;
            }
        }

        [Fact]
        public void GetOfferItems_DoNotIgnoreDistinctPTC()
        {
            int offerItemId = 1;
            var requestData = TestDataProvider.GetTestData<ReshopRequest>("OfferMS\\OrderReshopDuplicatePtcCheck");
            var itaRequest = TestDataProvider.GetXmlTestData<search>("OfferMS\\ITAReshopRequest");
            var result = TestDataProvider.GetXmlTestData<result>("ITAService\\Response\\Reshop_Response__Without_Repeting_PTC");
            var orderReshopResponse = ReshopBundleResponseMapper.ReshopResponseMapper(itaRequest, result, requestData, _cachedPriceClass.Object, _cachedCabin.Object, docID, DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat), _cachedPieceAllowance.Object, _cachedCabinBaggage.Object, _cachedAirportDetails.Object).Result;
            var offerItems = GetAllOfferItems(orderReshopResponse);
            var bundledSolnSummary = result.Items.FirstOrDefault(x => x.GetType() == typeof(resultBundledSolutionSummary)) as resultBundledSolutionSummary;
            foreach (var soln in bundledSolnSummary.solution)
            {
                foreach (var pricing in soln.pricing)
                {
                    var searhString = $"{docID}{Constants.Application.Pipeline}{soln.id}-{offerItemId++}";
                    Assert.NotNull(offerItems.FirstOrDefault(x => x.OfferItemId.Contains(searhString)));
                }
                offerItemId = 1;
            }
        }
        private List<OfferItem> GetAllOfferItems(OfferResponse reshopResponse)
        {
            List<OfferItem> offerList = new();
            foreach (var item in reshopResponse.Offers)
            {
                offerList.AddRange(item.OfferItems);
            }
            return offerList;
        }

    }
}

