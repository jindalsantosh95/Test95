using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopCancel;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshopCancel
{
    public class OrderReshopCancelQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderReshopCancelQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderReshopCancelQueryHandler>();
                })
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddTransient<IGenericDataMapper, GenericDataMapper>()
                .AddSingleton<OrderReshopCancelQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderReshopCancelQueryHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task MapToErrorOrderReshopRS_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopCancelRQ");
            request.DistributionChain.First().ParticipatingOrg = null;
            var subscriptionKey = "1234";

            //Act
            var _handler = _serviceProvider.GetService<OrderReshopCancelQueryHandler>();
            var result = await _handler.Handle(new OrderReshopCancelQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
            Assert.NotNull(result);
        }
    }
}
