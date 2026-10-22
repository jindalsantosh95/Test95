using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class ManageMultipleInfantAir4CommandHandlerTest
    {
        private readonly Mock<ICacheConfig> _cacheConfig;
        private readonly Mock<IAir4PassengerV4> _iAir4PassengerV4;
        private readonly ManageMultipleInfantAir4CommandHandler _handler;

        public ManageMultipleInfantAir4CommandHandlerTest()
        {
            _iAir4PassengerV4 = new Mock<IAir4PassengerV4>();
            _cacheConfig = new Mock<ICacheConfig>();
            _handler = new ManageMultipleInfantAir4CommandHandler(_iAir4PassengerV4.Object, _cacheConfig.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAir4IsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ManageMultipleInfantAir4CommandHandler(null, _cacheConfig.Object));
        }


        [Fact]
        public async Task HandlerTest_Success()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ManageMultipleInfantAir4Command>("PassengerMS\\ManageMultipleTravelDocsAndSecureFlightRequest");

            var response = new Infrastructure.Proxy.ManageMultipleInfantV2Response()
            {
                ManageMultipleInfantV2Response1 = new List<Infrastructure.Proxy.InfantV2ResponseInfoType>()
                {
                    new Infrastructure.Proxy.InfantV2ResponseInfoType() {
                        SecureFlightPassengerData = new Infrastructure.Proxy.SecureFlightPassengerDataType1() { SurName = "Test" },
                        GovtIssuedNumber=new List<Infrastructure.Proxy.GovtIssuedNumberType1>()
                        {
                             new Infrastructure.Proxy.GovtIssuedNumberType1()
                        {
                             AuthorizationToRetainGovtNum ="Test",
                             DocumentApplicableCountry ="Test",
                             DocumentApplicableState ="Test",
                             DocumentIssueDay ="Test",
                             DocumentIssueMonth ="Test",
                             DocumentIssueYear ="Test",
                             GovtIssuedNumber ="Test",
                             GovtIssuedNumberType ="Test",
                             InfantIndicator ="Test",
                             PlaceOfBirth ="Test",
                             PlaceOfIssue ="Test",
                             SourceOfGovtNumberOnHost ="Test",
                                        }
                        }.ToArray(),
                        InfantSSRFlightData= new List<Infrastructure.Proxy.InfantSSRFlightDataType>()
                        {
                            new Infrastructure.Proxy.InfantSSRFlightDataType()
                            {
                                    AirlineCode ="Test",
                                    ArrivalCityCode ="Test",
                                    ClassOfService ="Test",
                                    DepartureCityCode ="Test",
                                    FlightDate ="Test",
                                    FlightNumber ="Test",
                                    FlightSegmentNumber ="Test",
                                    INFTSSRActionIndicator ="Test",
                                    InfantSSRFreeFromText ="Test",
                                    InfantSSRPresenceIndicator ="Test",
                                    InventoryCount ="Test",
                                    ScheduleArrivalTime ="Test",
                                    ScheduleArrivalTimeIndicator ="Test",
                                    ScheduleDepartureTime ="Test",
                                    ScheduleDepartureTimeIndicator ="Test",
                                    SegmentStatus ="Test",
    }
                        }.ToArray(),
                        SeatEligiblePassengerInfo=new List<Infrastructure.Proxy.SeatEligiblePassengerInfoType>()
                        {
                            new Infrastructure.Proxy.SeatEligiblePassengerInfoType()
                            {
                                 InfantCarryEligibleInd ="Test",

                                NonRevenuePassengerInd ="Test",

                                PassengerGivenName ="Test",

                                PassengerGivenNameNumber ="Test",

                                PassengerSurname ="Test",

                                PassengerSurnameNumber ="Test",

                                ReasonForNonEligibility ="Test"
    }
                        }.ToArray(),
                        ManageInfantSuccess=true,
                        ManageInfantSuccessSpecified=false
                    }
                }.ToArray()
            };

            _cacheConfig.Setup(r => r.GetChannelEntities()).Returns(new List<ChannelEntity>() {
                TestDataProvider.GetTestData<ChannelEntity>("PassengerMS\\ChannelEntityData")
            });

            _iAir4PassengerV4.Setup(i => i.Air4ManageMultipleInfant(It.IsAny<Infrastructure.Proxy.ManageMultipleInfantV2RequestType>())).ReturnsAsync(response);

            //Act
            var result = await _handler.Handle(request, System.Threading.CancellationToken.None);

            //Assert

            Assert.NotNull(result.ManageMultipleInfantV2Response1.FirstOrDefault().SecureFlightPassengerData.SurName);
            Assert.Equal("Test", result.ManageMultipleInfantV2Response1.FirstOrDefault().GovtIssuedNumber.FirstOrDefault().PlaceOfIssue);
        }


    }
}
