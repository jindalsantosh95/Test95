using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class MVTStorageEventHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public MVTStorageEventHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "testConnectionString");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMVTStorageEventHandler, MVTStorageEventHandler>()
                .AddScoped(
                    typeof(ITableStorageService<>),
                    typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_ValidMessage_ReturnsSuccessMessage()
        {
            var handler = _serviceProvider.GetRequiredService<IMVTStorageEventHandler>();

            var message = new MVTMessageRequest
            {
                FlightCarrier = "VS",
                FlightNumber = 301,
                DepartureDate = "2026-08-06",
                DepartureAirport = "LHR",
                ArrivalAirport = "JFK"
            };

            var result = await handler.HandleAsync(message);

            Assert.Equal("Flight movement data processed successfully.", result);
        }

        [Fact]
        public async Task HandleAsync_NullMessage_ThrowsException()
        {
            var handler = _serviceProvider.GetRequiredService<IMVTStorageEventHandler>();

            var ex = await Assert.ThrowsAsync<Exception>(() => handler.HandleAsync(null));

            Assert.Contains("Error processing flight movement data", ex.Message);
        }
    }
}
