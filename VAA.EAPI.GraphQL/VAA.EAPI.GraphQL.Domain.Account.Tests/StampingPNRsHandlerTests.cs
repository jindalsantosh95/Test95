using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Account.Handlers;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class StampingPNRsHandlerTests
    {
        private const string FutureBookingsSinglePnrUrl = "https://Testresources.virginatlantic.com/dev/orderdata/future-bookings";
        private const string FutureBookingsEmptyUrl = "https://Testresources.virginatlantic.com/dev/orderdata/future-bookings-empty";

        private const string OrderSearchEligibleBaseUrl = "https://Testresources.virginatlantic.com/dev/ordersearchms-eligible/";
        private const string OrderSearchIneligibleBaseUrl = "https://Testresources.virginatlantic.com/dev/ordersearchms-ineligible/";

        private const string OrderManagementBaseUrl = "https://Testresources.virginatlantic.com/dev/ordermanagement/";
        private const string OrderManagementStampPnrUrl = OrderManagementBaseUrl + "AddFFP/StampFCNumberToPNR";

        private readonly IServiceProvider _serviceProvider;

        public StampingPNRsHandlerTests()
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                                                      .AddSingleton<IPnrQueryProvider, PnrQueryProvider>()
                                                      .AddSingleton<IPnrDetailsProvider, PnrDetailsProvider>()
                                                      .AddSingleton<IPnrUpdateMutationsProvider, PnrUpdateMutationsProvider>()
                                                      .AddSingleton<StampingPNRsHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenPnrQueryProviderIsNull()
        {
            var tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
            var connector = _serviceProvider.GetRequiredService<IHttpConnector>();

            Assert.Throws<ArgumentNullException>(() => new StampingPNRsHandler(null!, new PnrDetailsProvider(tokenProvider, connector), new PnrUpdateMutationsProvider(tokenProvider, connector)));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenPnrDetailsProviderIsNull()
        {
            var tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
            var connector = _serviceProvider.GetRequiredService<IHttpConnector>();

            Assert.Throws<ArgumentNullException>(() => new StampingPNRsHandler(new PnrQueryProvider(tokenProvider, connector), null!, new PnrUpdateMutationsProvider(tokenProvider, connector)));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenPnrUpdateMutationsProviderIsNull()
        {
            var tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
            var connector = _serviceProvider.GetRequiredService<IHttpConnector>();

            Assert.Throws<ArgumentNullException>(() => new StampingPNRsHandler(new PnrQueryProvider(tokenProvider, connector), new PnrDetailsProvider(tokenProvider, connector), null!));
        }

        [Fact]
        public async Task StampPNRs_ShouldStampPnr_WhenEligiblePassengerFound()
        {
            ConfigureStampingEndpoints(orderSearchMsUrl: OrderSearchEligibleBaseUrl, orderFutureBookingUrl: FutureBookingsSinglePnrUrl);

            var httpClientFactory = (HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var stampingPNRsHandler = _serviceProvider.GetRequiredService<StampingPNRsHandler>()!;
            var expectedStampUri = new Uri(OrderManagementStampPnrUrl);
            var customer = BuildCustomerDto(firstName: "John", lastName: "Doe");

            await stampingPNRsHandler.StampPNRs(customer);

            var responses = httpClientFactory.GetResponses(OrderSearchEligibleBaseUrl)
                                             .Where(r => r.RequestMessage?.Method == HttpMethod.Patch
                                                      && r.RequestMessage.RequestUri is not null
                                                      && r.RequestMessage.RequestUri.Scheme.Is(expectedStampUri.Scheme)
                                                      && r.RequestMessage.RequestUri.Host.Is(expectedStampUri.Host)
                                                      && r.RequestMessage.RequestUri.AbsolutePath.Is(expectedStampUri.AbsolutePath))
                                             .ToList();

            var calls = string.Join(", ", responses.Select(r => $"{r.RequestMessage?.Method} {r.RequestMessage?.RequestUri}"));

            Assert.True(responses.Count == 1, $"Expected exactly 1 stamping call but found {responses.Count}. Calls: {calls}");
        }

        [Fact]
        public async Task StampPNRs_ShouldNotStampPnr_WhenPnrAlreadyHasFfpNumber()
        {
            ConfigureStampingEndpoints(orderSearchMsUrl: OrderSearchIneligibleBaseUrl, orderFutureBookingUrl: FutureBookingsSinglePnrUrl);

            var httpClientFactory = (HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var stampingPNRsHandler = _serviceProvider.GetRequiredService<StampingPNRsHandler>()!;
            var expectedStampUri = new Uri(OrderManagementStampPnrUrl);
            var customer = BuildCustomerDto(firstName: "John", lastName: "Doe");

            await stampingPNRsHandler.StampPNRs(customer);

            var responses = httpClientFactory.GetResponses(OrderSearchIneligibleBaseUrl)
                                             .Where(r => r.RequestMessage?.Method == HttpMethod.Patch
                                                      && r.RequestMessage.RequestUri is not null
                                                      && r.RequestMessage.RequestUri.Scheme.Is(expectedStampUri.Scheme)
                                                      && r.RequestMessage.RequestUri.Host.Is(expectedStampUri.Host)
                                                      && r.RequestMessage.RequestUri.AbsolutePath.Is(expectedStampUri.AbsolutePath))
                                             .ToList();

            Assert.Empty(responses);
        }

        [Fact]
        public async Task StampPNRs_ShouldNotStampPnr_WhenNoPnrsReturned()
        {
            ConfigureStampingEndpoints(orderSearchMsUrl: OrderSearchEligibleBaseUrl, orderFutureBookingUrl: FutureBookingsEmptyUrl);

            var httpClientFactory = (HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()!;
            var stampingPNRsHandler = _serviceProvider.GetRequiredService<StampingPNRsHandler>()!;
            var expectedStampUri = new Uri(OrderManagementStampPnrUrl);
            var customer = BuildCustomerDto(firstName: "John", lastName: "Doe");

            await stampingPNRsHandler.StampPNRs(customer);

            var responses = httpClientFactory.GetResponses(OrderSearchEligibleBaseUrl)
                                             .Where(r => r.RequestMessage?.Method == HttpMethod.Patch
                                                      && r.RequestMessage.RequestUri is not null
                                                      && r.RequestMessage.RequestUri.Scheme.Is(expectedStampUri.Scheme)
                                                      && r.RequestMessage.RequestUri.Host.Is(expectedStampUri.Host)
                                                      && r.RequestMessage.RequestUri.AbsolutePath.Is(expectedStampUri.AbsolutePath))
                                             .ToList();

            Assert.Empty(responses);
        }

        private static void ConfigureStampingEndpoints(string orderSearchMsUrl, string orderFutureBookingUrl)
        {
            ConfigReader.Set(Environment.OrderFutureBookingUrl, orderFutureBookingUrl);
            ConfigReader.Set(Environment.OrderDataAppId, "test-orderdata-appid");
            ConfigReader.Set(Environment.OrderDataOcpKey, "test-orderdata-ocp");

            ConfigReader.Set(Environment.OrderSearchMSUrl, orderSearchMsUrl);
            ConfigReader.Set(Environment.OrderSearchAppId, "test-ordersearch-appid");
            ConfigReader.Set(Environment.OrderSearchOcpKey, "test-ordersearch-ocp");

            ConfigReader.Set(Environment.StampingFCNumberToPNRUrl, OrderManagementBaseUrl);
            ConfigReader.Set(Environment.OrderManagementAppId, "test-ordermgmt-appid");
            ConfigReader.Set(Environment.OrderManagementOcpKey, "test-ordermgmt-ocp");
        }

        private static CustomerDTO BuildCustomerDto(string firstName, string lastName)
        {
            return new CustomerDTO
            {
                Customer = new CustomerCustMS
                {
                    FirstName = firstName,
                    LastName = lastName,
                    LoyaltyNumber = "1501033656",
                    ContactDetails = new Contact
                    {
                        EmailDetails = new List<Email>
                        {
                            new() { PreferredFlag = "Y", emailId = "john.doe@example.com" }
                        }
                    }
                }
            };
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}
