using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.UpdateReservation
{
    public class UpdatePassengerNameRecordMapperTests
    {
        public UpdatePassengerNameRecordMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void GetUpdateReservationRQMapperForSFPD_Returns_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt/OrderChangeRQForSFPD");
            var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationInterLineSegments");
            var passengers = reservationRS.Item as EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB;
            ConfigReader.Set("InterLineOperatingSegments", "BW");

            //Act
            var result = UpdatePassengerNameRecordMapper.UpdatePnrReqMapper(token, request, passengers, true);
            var secureFlight = result.UpdatePassengerNameRecordRQ.SpecialReqDetails.SpecialService.SpecialServiceInfo.SecureFlight;

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(secureFlight);
            Assert.Equal("1", secureFlight.Count().ToString());
        }
    }
}
