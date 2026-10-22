using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests;

public class OfferITARequestMapperTests
{
    [Fact]
    public void OfferSearchReqMapper_UsesNormalItaValues()
    {
        OfferTestsCommon.CreateTestEnvironmentSettings();

        var offerRequest = TestDataProvider.GetTestData<OfferRequest>("OfferMS\\OfferRequest");
        offerRequest.Parties.Participants.First().AccountCode = "normal-account-code";

        var result = OfferITARequestMapper.OfferSearchReqMapper(offerRequest, true);

        Assert.Equal(searchDataset.production, result.dataset);
        Assert.Equal("normal-account-code", Assert.Single(result.inputs.accountCode));
        Assert.True(result.inputs.requirePrivateFares);
    }

    [Fact]
    public void OfferSearchReqMapper_UsesConfiguredItaDataset()
    {
        OfferTestsCommon.CreateTestEnvironmentSettings();
        ConfigReader.Set(Constants.Environment.ItaDataset, nameof(searchDataset.experimental));

        try
        {
            var offerRequest = TestDataProvider.GetTestData<OfferRequest>("OfferMS\\OfferRequest");

            var result = OfferITARequestMapper.OfferSearchReqMapper(offerRequest, true);

            Assert.Equal(searchDataset.experimental, result.dataset);
        }
        finally
        {
            ConfigReader.Set(Constants.Environment.ItaDataset, nameof(searchDataset.production));
        }
    }
}
