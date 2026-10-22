using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests
{
    public class StoreAppointmentEmailPublisherTests
    {
        private readonly IServiceProvider _serviceProvider;

        public StoreAppointmentEmailPublisherTests()
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<MessageBusFactory>()
                                                      .AddSingleton<IMessageBusFactory>(provider => provider.GetRequiredService<MessageBusFactory>())
                                                      .AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<StoreAppointmentEmailPublisher>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task PublishAsync_ShouldIncludeAppointmentReferenceIdInCustomerAndStoreEmailXml()
        {
            var messageBusFactory = _serviceProvider.GetRequiredService<MessageBusFactory>();
            var publisher = _serviceProvider.GetRequiredService<StoreAppointmentEmailPublisher>();
            var deliveredEvents = new List<VAA.EAPI.Domain.Events.CloudEvent>();

            const string serviceBusConnectionString = "Endpoint=sb://unit-test/";
            var messageBus = (MessageBus)messageBusFactory.GetClient(serviceBusConnectionString, Constants.Constants.StoreAppointmentTopicName);
            messageBus.OnMessage += deliveredEvents.Add;

            var request = new AppointmentBookingRequest
            {
                Customer = new BookingCustomer
                {
                    FirstName = "Jane",
                    Email = "jane@example.com"
                }
            };

            var store = new StoreResult
            {
                Label = "Oxford Circus",
                Name = "Oxford Circus Store",
                FullDetails = new StoreInformation
                {
                    Details = new StoreContactDetails
                    {
                        Address = new StoreAddress
                        {
                            AddressLine1 = "10 High St",
                            City = "London",
                            PostalCode = "W1"
                        },
                        EmailAddress = "store@example.com"
                    }
                }
            };

            var previousServiceBusConnection = Environment.GetEnvironmentVariable(Constants.Environment.StoreAppointmentServiceBusConnection);
            try
            {
                Environment.SetEnvironmentVariable(Constants.Environment.StoreAppointmentServiceBusConnection, serviceBusConnectionString);

                await publisher.PublishAsync("appointment-1", request, store);
            }
            finally
            {
                Environment.SetEnvironmentVariable(Constants.Environment.StoreAppointmentServiceBusConnection, previousServiceBusConnection);
                messageBus.OnMessage -= deliveredEvents.Add;
            }

            Assert.Equal(2, deliveredEvents.Count);

            var customerEventData = Assert.IsType<StoreAppointmentEmailEventData>(deliveredEvents[0].Data);
            var customerXml = XElement.Parse(customerEventData.EmailDataXml);
            var storeEventData = Assert.IsType<StoreAppointmentEmailEventData>(deliveredEvents[1].Data);
            var storeXml = XElement.Parse(storeEventData.EmailDataXml);
            Assert.Equal(
                "10 High St, London, W1",
                customerXml.Element("LocationTextForAddCalendar")?.Value);
            Assert.Equal(
                "appointment-1",
                customerXml.Element("AppointmentReferenceId")?.Value);
            Assert.Equal(
                "appointment-1",
                storeXml.Element("AppointmentReferenceId")?.Value);
        }
    }
}
