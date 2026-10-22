using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests.ManualPriceQuote
{
    public class ManualPriceQuoteRequestMapperTests
    {
        public ManualPriceQuoteRequestMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperCreationTests_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt/OrderChangeRequest");
            var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_1Pax");
            bool type = false;
            string index = "1";
            string ptcType = request.DataList.CustomerDetails.First().Ptc;

            //Act
            var passengers = ((ReservationPNRB)reservationRS.Item).PassengerReservation.Passengers.Passenger;
            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request, ptcType, index, passengers);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.CreationDetails);
            Assert.Equal("1", result.ManualPriceQuoteAddInfoRQ.CreationDetails.ItineraryOptions.First().Number);
            Assert.Contains("ADT", result.ManualPriceQuoteAddInfoRQ.CreationDetails.PassengerType);
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperContentTests_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\OrderchangeRequest");
            bool type = true;
            string index = "1";
            string ptcType = request.DataList.CustomerDetails.First().Ptc;

            //Act
            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request, ptcType, index, null);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("LON VS X/MSP VS ATL M1442.83BY2QBDLA NUC1442.83END ROE0.804668", result.ManualPriceQuoteAddInfoRQ.ContentDetails.FareCalc.Text);
            Assert.Equal("1161.00", result.ManualPriceQuoteAddInfoRQ.ContentDetails.FareLadder.BaseFare.Amount);
            Assert.Equal("GBP", result.ManualPriceQuoteAddInfoRQ.ContentDetails.FareLadder.BaseFare.CurrencyCode);
            Assert.Equal("BY2QBDLA", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().FareBasis.Code);
            Assert.Equal("1", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().ItineraryOptions.SegmentSelect.First().Number);
            Assert.Equal("X", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().ItineraryOptions.SegmentSelect.First().ConnectionInd);
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperContentTestsForXFFareBasisCode_PositiveResponse()
        {
            //Arrange
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRepoCommand>("SabreOrderChangeMgt\\OrderchangeRequestXFFareBasis");
            var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRSXFFareBasis");

            bool type = true;
            string index = "1";
            string ptcType = request.orderChangeRequestData.DataList.CustomerDetails.First().Ptc;

            //Act
            var passengers = ((ReservationPNRB)reservationRS.Item).PassengerReservation.Passengers.Passenger;
            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request.orderChangeRequestData, ptcType, index, passengers);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Security);
            Assert.NotNull(result.MessageHeader);
            Assert.Equal("FromPartyId", result.MessageHeader.From.PartyId.First().Value);
            Assert.Equal("ToPartyId", result.MessageHeader.To.PartyId.First().Value);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("LON VS X/E/AMS VS NYC M117.39 VS LON M24.42 NUC141.81END ROE0.736827 XFJFK4.5", result.ManualPriceQuoteAddInfoRQ.ContentDetails.FareCalc.Text);
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperContentShouldSetTicketDesignator_ForCorporateFare()
        {
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            bool type = true;
            string index = "1";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\OrderChangeReq_Corporate");
            string ptcType = request.DataList.CustomerDetails.First().Ptc;

            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request, ptcType, index, null);

            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("VS573", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().FareBasis.TicketDesignator);
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperContentShouldPadBaggageAllowanceNumber_WhenCheckedBagsExist()
        {
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\OrderchangeRequest");
            request.DataList.BaggageAllowances.First(x => x.BaggageAllowanceTypeCode == "Checked").FreeBaggageAllowance.AllowedBagCnt = 1;
            bool type = true;
            string index = "1";
            string ptcType = request.DataList.CustomerDetails.First().Ptc;

            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request, ptcType, index, null);

            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("01", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().BaggageAllowance.Number);
        }

        [Fact]
        public void ManualPriceQuoteRequestMapperContentShouldMapZeroBaggageAllowanceNumber_WhenNoCheckedBagsAllowed()
        {
            string token = "Shared/IDL:IceSess\\/SessMgr:1\\.0.IDL/Common/!ICESMS\\/ACPCRTD!ICESMSLB\\/CRT.LB!1736491182880!4762!545";
            var request = TestDataProvider.GetTestData<OrderChangeRequestData>("SabreOrderChangeMgt\\OrderchangeRequest");
            request.DataList.BaggageAllowances.First(x => x.BaggageAllowanceTypeCode == "Checked").FreeBaggageAllowance.AllowedBagCnt = 0;

            bool type = true;
            string index = "1";
            string ptcType = request.DataList.CustomerDetails.First().Ptc;

            var result = ManualPriceQuoteAddInfoRQMapper.ManualPriceQuoteAddInfoRQRequest(token, type, request, ptcType, index, null);

            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("0", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().BaggageAllowance.Number);
        }
    }
}
