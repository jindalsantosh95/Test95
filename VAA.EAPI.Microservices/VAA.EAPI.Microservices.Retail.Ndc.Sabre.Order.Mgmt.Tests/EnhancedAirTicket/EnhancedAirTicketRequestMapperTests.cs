using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.EnhancedAirTicket
{
    public class EnhancedAirTicketRequestMapperTests
    {
        public EnhancedAirTicketRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void GetEnhancedAirTicketRQMapper_ReturnPositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt/OrderChangeRequest");
            var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_1Pax");
            ConfigReader.Set("DirectTicketingPCC", "0ABC,0BD4");

            //Act
            var pricedItinerary = ((ReservationPNRB)reservationRS.Item).PassengerReservation.ItineraryPricing.PricedItinerary;
            var result = EnhancedAirTicketRequestMapper.GetEnhancedAirTicketRequest(token,request.Rloc, pricedItinerary, "09UI");

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("AirTicketRQ", result.MessageHeader.Action);
            Assert.Equal("ConversationId", result.MessageHeader.ConversationId);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
            Assert.Equal("OVPZGG", result.AirTicketRQ.Itinerary.ID);
            Assert.Equal("3", result.AirTicketRQ.Ticketing.First().PricingQualifiers.PriceQuote.First().Record.First().Number);
            Assert.True(result.AirTicketRQ.Ticketing.First().PricingQualifiers.PriceQuote.First().Record.First().Reissue);
            Assert.Equal("API", result.AirTicketRQ.PostProcessing.EndTransaction.Source.ReceivedFrom);
        }
    }
}
