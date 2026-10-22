using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.OTACancel
{
    public class OTACancelLLSRequestMapperTests
    {
        public OTACancelLLSRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void OTACancelLLSRequestMapper_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt/OrderChangeRequest");

            //Act
            var result = OTACancelLLSRequestMapper.GetOTACancelRequest(token, request.OrderAmendments.First().FlightItems);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.OTA_CancelRQ);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("1", result.OTA_CancelRQ.Segment.First().Number);
        }
    }
}
