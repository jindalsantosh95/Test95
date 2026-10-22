using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation;
using Xunit;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.ReshopForCancellation;
using VAA.EAPI.Common.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class ReshopForCancellationResponseMapperTests
    {
        private readonly Mock<IMediator> _mediator;
        public ReshopForCancellationResponseMapperTests()
        {
            _mediator = new Mock<IMediator>();
            ConfigReader.Set("OfferExpiryLimitInMinutes", "30");
        }

        [Fact]
        public void ValidResponseMappingTests()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ReshopForCancellationQuery>("OfferMS\\OrderReshopCancellationValidRequest");

            var responseData= ReshopForCancellationResponseMapper.RfcReshopResponse(request, DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat));
            var responseValue = responseData.Offers.First().OfferItems.First().OfferitemPricing;

            //Assert
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Offers.First().OfferExpirationTimeLimit);
            Assert.Equal(4, responseData.DataList.CustomerDetails.Count());
            Assert.Equal("ADT", responseData.DataList.CustomerDetails.First().Ptc);
            Assert.Equal("GBE", responseData.DataList.CustomerDetails.ElementAt(1).Ptc);
            Assert.Equal("INF", responseData.DataList.CustomerDetails.Last().Ptc);
            Assert.Equal("GBP", responseValue.First().OriginalSalePrice.First().Amount.BaseCurrency);
        }
    }
}
