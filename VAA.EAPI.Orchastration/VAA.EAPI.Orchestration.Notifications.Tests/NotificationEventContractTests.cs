using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using VAA.EAPI.Orchestration.Notifications.Mapper;
using Notification = VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;
using NotificationEnvironment = VAA.EAPI.Orchestration.Notifications.Constants.Environment;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class NotificationEventContractTests
    {
        public NotificationEventContractTests()
        {
            ConfigReader.Set(NotificationEnvironment.NotificationTopicName, NotificationConstants.NotificationTopicName);
            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public void Constants_Should_Match_ServiceBus_Behavior_Contracts()
        {
            Assert.Equal("servicenotifications", NotificationConstants.NotificationTopicName);
            Assert.Equal("CheckinOpen", NotificationConstants.CheckinOpenSubscriptionName);
            Assert.Equal("StarlinkEnabledFlight", NotificationConstants.StarlinkEnabledFlightSubscriptionName);
            Assert.Equal("FlightOrderData", NotificationConstants.FlightOrderDataSubscriptionName);
            Assert.Equal("CheckinCustProfileData", NotificationConstants.CheckinCustProfileDataSubscriptionName);
            Assert.Equal("Orches.Notif.StarlinkEnabledFlight", NotificationConstants.StarlinkEnabledFlightEventSubject);
            Assert.Equal("Retail.Order.data.FlightOrderDataRequest", NotificationConstants.FlightOrderDataReqEventSubject);
            Assert.Equal("Crm.CP.CheckinFlightOrderCustProfileRequest", NotificationConstants.CheckinFlightCustProfileReqEventSubject);
            Assert.Equal("Crm.Notif.FlightOrderNotifRequest", NotificationConstants.FlightOrderNotifReqEventSubject);
            Assert.Equal("Starlink", NotificationConstants.NotificationTypeStarlink);
        }

        [Fact]
        public void FlightOrderDataMapper_Should_Emit_Order_Request_Contract()
        {
            var request = new FlightOrderDataRequest
            {
                Departure = "ATL",
                Arrival = "LHR",
                OperatingCarrier = new Carrier
                {
                    CarrierCode = "VS",
                    FlightNum = "317"
                },
                ScheduledDepartureUtcDateTime = new DateTime(2025, 09, 29, 14, 30, 00),
                ScheduledDepartureLocalDateTime = new DateTime(2025, 09, 29, 10, 30, 00)
            };

            var cloudEvent = NotifCloudEventMapper.FlightOrderDataEventMapper(request, "event-1");

            Assert.Equal("event-1", cloudEvent.Id);
            Assert.Equal(NotificationConstants.FlightOrderDataReqEventSubject, cloudEvent.Type);
            Assert.Equal(NotificationConstants.FlightOrderDataReqEventSubject, cloudEvent.Subject);
            Assert.Equal(NotificationConstants.NotificationTopicName, cloudEvent.Source);
            Assert.Equal("1.0", cloudEvent.Specversion);

            var data = Assert.IsType<FlightOrderDataRequest>(cloudEvent.Data);
            Assert.Equal("317", data.OperatingCarrier.FlightNum);
            Assert.Equal("VS", data.OperatingCarrier.CarrierCode);
            Assert.Equal("ATL", data.Departure);
            Assert.Equal("LHR", data.Arrival);
            Assert.Equal(NotificationConstants.NotificationTypeCheckInOpen, data.NotificationType);
        }

        [Fact]
        public void StarlinkFlightOrderDataMapper_Should_Emit_Starlink_Order_Request_Contract()
        {
            var flight = new StarlinkFlightDetail
            {
                Departure = "ATL",
                Arrival = "LHR",
                Carrier = "VS",
                FlightNo = 317,
                Date = new DateOnly(2025, 09, 29),
                ScheduledTimeOfDeparture = new TimeOnly(14, 30, 00),
                TailNumber = "G-VTEA"
            };

            var request = (FlightOrderDataRequest)flight;

            Assert.Equal("317", request.OperatingCarrier.FlightNum);
            Assert.Equal("VS", request.OperatingCarrier.CarrierCode);
            Assert.Equal("ATL", request.Departure);
            Assert.Equal("LHR", request.Arrival);
            Assert.Equal(new DateTime(2025, 09, 29, 14, 30, 00), request.ScheduledDepartureUtcDateTime);
            Assert.Equal(NotificationConstants.NotificationTypeStarlink, request.NotificationType);
        }

        [Fact]
        public void NotificationMapper_Should_Compress_Notification_Request_Contract()
        {
            var request = new Notification.NotificationRequest
            {
                Message = "You are now ready to check in for your flight to London Heathrow from Atlanta.",
                DeepLink = "https://virginatlantic.com/trips",
                FlightOrderData =
                [
                    new()
                    {
                        EventFlight = new()
                        {
                            OperatingCarrier = new()
                            {
                                CarrierCode = "VS",
                                FlightNum = "317"
                            },
                            ScheduledDepartureLocalDateTime = new DateTime(2025, 09, 29, 10, 30, 00)
                        },
                        BookingReference = new()
                        {
                            AirlineDesignator = "VA",
                            PnrLocator = "PNR-VA317",
                            CreatedDate = new DateTime(2025, 07, 24, 14, 30, 00)
                        }
                    }
                ]
            };

            var cloudEvent = NotifCloudEventMapper.NotifEventMapper(request, "event-2");

            Assert.Equal("event-2", cloudEvent.Id);
            Assert.Equal(NotificationConstants.FlightOrderNotifReqEventSubject, cloudEvent.Type);
            Assert.Equal(NotificationConstants.FlightOrderNotifReqEventSubject, cloudEvent.Subject);
            Assert.Equal(NotificationConstants.NotificationTopicName, cloudEvent.Source);

            var compressedData = Assert.IsType<byte[]>(cloudEvent.Data);
            var notification = compressedData.DecompressTo<Notification.NotificationRequest>();

            Assert.Equal(request.Message, notification.Message);
            Assert.Equal(request.DeepLink, notification.DeepLink);
            Assert.Equal("PNR-VA317", notification.FlightOrderData.First().BookingReference.PnrLocator);
        }
    }
}
