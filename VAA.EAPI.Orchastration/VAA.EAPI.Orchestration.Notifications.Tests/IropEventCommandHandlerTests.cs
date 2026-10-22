using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Orchestration.Notifications.Application.Events;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop;
using NotificationConstants = VAA.EAPI.Orchestration.Notifications.Constants.Constants;
using NotificationEnvironment = VAA.EAPI.Orchestration.Notifications.Constants.Environment;

namespace VAA.EAPI.Orchestration.Notifications.Tests
{
    public sealed class IropEventCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Track_Irop_Notification_In_Analytics()
        {
            ConfigReader.Set(NotificationEnvironment.FlightCancelledContent, "https://tempuri.org/flight-cancelled");
            ConfigReader.Set(NotificationEnvironment.FlightCancelledAlertMessage, "Your flight has been cancelled.");

            var analyticsEventTracker = new TestAnalyticsEventTracker();
            var handler = CreateHandler(analyticsEventTracker);

            await handler.Handle(CreateIropMessage(), TestContext.Current.CancellationToken);

            Assert.Equal(NotificationConstants.IropNotificationReceivedTelemetryEvent, analyticsEventTracker.Event.Subject);
            Assert.Equal(nameof(TrackingAnalyticsType.Microservice), analyticsEventTracker.Event.Type);
            Assert.Equal("PNR-VA317", analyticsEventTracker.Event.GetValue("PnrLocator"));
            Assert.Equal("CANCL", analyticsEventTracker.Event.GetValue("ReasonType"));
        }

        [Fact]
        public async Task Handle_Should_Ignore_Message_Without_Irop_Notification()
        {
            var analyticsEventTracker = new TestAnalyticsEventTracker();
            var handler = CreateHandler(analyticsEventTracker);

            await handler.Handle(new Ctx
            {
                SendMessage = new()
                {
                    MessageData = new()
                }
            }, TestContext.Current.CancellationToken);

            Assert.Equal(0, analyticsEventTracker.CallCount);
        }

        private static IIropEventCommand CreateHandler(TestAnalyticsEventTracker analyticsEventTracker)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testTable");
            ConfigReader.Set(NotificationEnvironment.NotificationServiceBusConnectionString, "testConnectionString");
            ConfigReader.Set(NotificationEnvironment.NotificationTopicName, NotificationConstants.NotificationTopicName);

            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(analyticsEventTracker)
                .AddSingleton<IIropEventCommand, IropEventCommandHandler>()
                .BuildServiceProvider()
                .GetRequiredService<IIropEventCommand>();
        }

        private static Ctx CreateIropMessage() => new()
        {
            SendMessage = new()
            {
                MessageData = new()
                {
                    ENENotification = new()
                    {
                        ReasonType = "CANCL",
                        RecordLocator = "PNR-VA317",
                        BookingCreationGMTDate = new DateTime(2025, 09, 29),
                        Flights = new()
                        {
                            ENEFlight = new()
                            {
                                MarketedFlightNumber = 317
                            }
                        }
                    }
                }
            }
        };

        private sealed class TestAnalyticsEventTracker : ITrackAnalyticsEventCommandHandler
        {
            public int CallCount { get; private set; }
            public TrackAnalyticsEventCommand Event { get; private set; }

            public Task<bool> Handle(TrackAnalyticsEventCommand request, CancellationToken cancellationToken)
            {
                CallCount++;
                Event = request;
                return Task.FromResult(true);
            }
        }
    }
}
