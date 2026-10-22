using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.EnhancedAirBook
{
    public class EnhancedAirBookRequestMapperTests
    {
        public EnhancedAirBookRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void GetEnhancedAirBookRQMapper_ReturnPositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt/OrderChangeRequest");

            //Act
            var result = EnhancedAirBookRequestMapper.GetEnhancedAirBookRequest(token, request, request.DataList.CustomerDetails);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.EnhancedAirBookRQ);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
            Assert.Equal("2025-11-19T14:55", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().ArrivalDateTime);
            Assert.Equal("2025-11-19T12:10", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().DepartureDateTime);
            Assert.Equal("4041", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().FlightNumber);
            Assert.Equal("1", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().NumberInParty);
            Assert.Equal("T", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().ResBookDesigCode);
            Assert.Equal("4041", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().MarketingAirline.FlightNumber);
            Assert.Equal("VS", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().MarketingAirline.Code);
            Assert.Equal("LHR", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().OriginLocation.LocationCode);
            Assert.Equal("MSP", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().DestinationLocation.LocationCode);
            Assert.Equal("NN", result.EnhancedAirBookRQ.OTA_AirBookRQ.OriginDestinationInformation.First().Status);
            Assert.Equal("1000", result.EnhancedAirBookRQ.PostProcessing.RedisplayReservation.WaitInterval);
        }
    }
}
