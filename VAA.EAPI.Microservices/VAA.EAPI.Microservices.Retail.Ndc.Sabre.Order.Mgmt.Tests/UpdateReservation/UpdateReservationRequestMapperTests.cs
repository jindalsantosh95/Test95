using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.UpdateReservation
{
    public class UpdateReservationRequestMapperTests
    {
        public UpdateReservationRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]

        public void GetUpdateReservationRQMapper_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<ManageCustomerRequest>("SabreOrderChangeMgt/OrderChangeAmendApisRQ");
            var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationAmendApisRS");

            var passengers = (reservationRS.Item as EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB).PassengerReservation.Passengers.Passenger;

            //Act
            var result = UpdateReservationRequestMapper.GetUpdateReservationRequest(token, request, passengers);
            var UpdateItem = ((ReservationUpdateItemType)result.UpdateReservationRQ.ReservationUpdateList.Items.First());
            var reservationUpdateItem = ((EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation.SpecialServiceRequestUpdatePNRB)UpdateItem.Items.First());

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.UpdateReservationRQ);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.Equal(token, result.Security.BinarySecurityToken);
            Assert.Equal("1", UpdateItem.UpdateId);
            Assert.Equal(VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation.OperationTypePNRB.D, reservationUpdateItem.op);
            Assert.Equal("9", reservationUpdateItem.id);
        }
    }
}
