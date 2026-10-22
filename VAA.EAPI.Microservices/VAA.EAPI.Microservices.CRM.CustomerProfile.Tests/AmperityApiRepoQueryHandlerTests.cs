using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Tests
{
    public class AmperityApiRepoQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AmperityApiRepoQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                                     .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                     .AddSingleton<IHttpConnector, HttpConnector>()
                                                     .AddSingleton<IAmperityApiRepoQuery, AmperityApiRepoQueryHandler>()
                                                     .BuildServiceProvider();
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AmperityApiRepoQueryHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldAssignAmperityId_AndReturnSuccessResponse()
        {
            // Arrange
            var _amperityApiRepoQueryHandler = _serviceProvider.GetService<IAmperityApiRepoQuery>()!;
            var request = new CustomerProfileRequest
            {
                Passenger = new Passenger { FirstName = "felix", LastName = "raja", PaxType = "ADT" }
            };

            // Act
            var result = await _amperityApiRepoQueryHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.False(string.IsNullOrEmpty(result.Value.AmperityId));
            Assert.Equal(request.Passenger.FirstName, result.Value.FirstName);
            Assert.Equal(request.Passenger.LastName, result.Value.LastName);
            Assert.Equal(request.Passenger.PaxType, result.Value.PaxType);
        }
    }
}
