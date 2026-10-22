using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using System.Reflection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class OrderReshopQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderReshopQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderReshopQueryHandler>();
                })
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddTransient<IGenericDataMapper, GenericDataMapper>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<OrderReshopQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderReshopQueryHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task MapToErrorOrderReshopRs_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopRQ");
            request.DistributionChain.First().ParticipatingOrg = null;
            var subscriptionKey = "1234";

            //Act
            var _handler = _serviceProvider.GetService<OrderReshopQueryHandler>();
            var result = await _handler.Handle(new OrderReshopQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task OrderReshopPartialFlownScenariosReturn_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderReshopRQ>("NDCOffer\\OrderReshop\\OrderReshopPartialFlownRQ");
            var subscriptionKey = "1234";

            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("ConfigurationMSOcpKey", "Test_ConfigurationMSOcpKey");
            ConfigReader.Set("ConfigurationMSReshopFareFamilyEndpoint", "https://testresources.virginatlantic.com/dev/ConfigurationMicroservice/Configurations/ReshopFareFamily");
            ConfigReader.Set("OrderSearchMSUrl", "https://testresources.virginatlantic.com/dev/NdcOrderSearch/");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey"); 
            ConfigReader.Set("BrandMSUrl", "https://testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("ReferenceMSOcpKey", "Test_ReferenceMSOcpKey");
            ConfigReader.Set("ReferenceMSAirportDetailsEndpoint", "https://testresources.virginatlantic.com/dev/ReferenceMicroservice/AirportDetails");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("OrderReshopEndpoint", "https://Testresources.virginatlantic.com/dev/OfferMicroservice/order/reshopbundle");

            //Act
            var _handler = _serviceProvider.GetService<OrderReshopQueryHandler>();
            var result = await _handler.Handle(new OrderReshopQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (OrderReshopResponseType[])result.Value.Value.Items;
            var reshopOffer = items.First().ReshopResults.Item as ReshopOffersType;
            Assert.NotNull(result);
            Assert.NotNull(reshopOffer);
            Assert.NotNull(reshopOffer.Offer.First().BaggageAssociations);
            Assert.Equal("BAGA1", reshopOffer.Offer.First().BaggageAssociations.First().BaggageAllowanceRefID.First());
        }
        [Fact]
        public void UpdateOrlandoCityCodeToMco_Valid_Input_Should_Update_CityCode()
        {
            // Arrange
            var airportDetails = new List<AirportDetail>
            {
             new() { AirportCode = "MCO", CityCode = "ORL" },
             new() { AirportCode = "MCO", CityCode = "MCO" },
             new() { AirportCode = "JFK", CityCode = "NYC" }
            };

            var methodObj = typeof(OrderReshopQueryHandler).GetMethod(
                "UpdateOrlandoCityCodeToMco",
                BindingFlags.NonPublic | BindingFlags.Static);

            var method = Assert.IsAssignableFrom<MethodInfo>(methodObj);

            method.Invoke(null, new object?[] { airportDetails });

            // Assert
            Assert.Equal("MCO", airportDetails[0].CityCode);
            Assert.Equal("MCO", airportDetails[1].CityCode);
            Assert.Equal("NYC", airportDetails[2].CityCode);
        }
    }
}
