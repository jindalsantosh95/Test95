using MediatR;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4TokenGenerator;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentReferenceIdGenerateCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHttpConnector _httpConnectorMock;
        private readonly Mock<IMediator> _mediator;
        private readonly ILogger<OrderPaymentReferenceIdGenerateCommandHandler> _logger;
        private readonly ILogger<OrderPaymentReferenceIdGenerateRepoCommandHandler> _repoLogger;
        private readonly ILogger<OrderPaymentReferenceIdGenerateServiceCommandHandler> _serviceLogger;

        private readonly IAir4TokenGenerator _tokenGenerator;
        private readonly ICachedChannel _cachedChannel;
        private readonly ITableStorageService<Channel> _tableStorageService;
        private readonly IMemoryCache _cachedChannelMemoryCache;
        private readonly OrderPaymentReferenceIdGenerateRepoCommandHandler _repoHandler;
        private readonly OrderPaymentReferenceIdGenerateServiceCommandHandler _serviceHandler;
        private readonly OrderPaymentReferenceIdGenerateCommandHandler _handler;


        public OrderPaymentReferenceIdGenerateCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            PRIGenerationEnvironmentSetup.CreateTestEnvironmentSettings();
            var httplogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentReferenceIdGenerateCommandHandler>.Instance;
            _repoLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentReferenceIdGenerateRepoCommandHandler>.Instance;
            _serviceLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentReferenceIdGenerateServiceCommandHandler>.Instance;
            _mediator = new Mock<IMediator>();
            _tableStorageService = new VAA.EAPI.Core.Xunit.Azure.TableStorageService<Channel>();
            _httpConnectorMock = new HttpConnector(httplogger, httpClientFactoryFixture.Current);
            _tokenGenerator = new Air4TokenGenerator(_httpConnectorMock);
            _cachedChannelMemoryCache = new MemoryCache(new MemoryCacheOptions());
            _cachedChannel = new CachedChannel(_tableStorageService, _cachedChannelMemoryCache);
            _repoHandler = new OrderPaymentReferenceIdGenerateRepoCommandHandler(_mediator.Object, _repoLogger);
            _serviceHandler = new OrderPaymentReferenceIdGenerateServiceCommandHandler(_serviceLogger, _httpConnectorMock, _tokenGenerator, _cachedChannel);

            _handler = new OrderPaymentReferenceIdGenerateCommandHandler(
                _mediator.Object,
                _cachedChannel
            );

            // Set a mock correlation ID in AsyncContext
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorisNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderPaymentReferenceIdGenerateCommandHandler(null, _cachedChannel));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCacheChannelisNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderPaymentReferenceIdGenerateCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_GetGenerateReferenceId_ExecuteSuccessfullyforBaggagePayment()
        {
            //Arrange
            var bagsPaymentRequest = new PaymentRefIdGenerateRequest
            {
                Pos = new EAPI.Domain.Virgin.Models.Common.Pos
                {
                    IpAddress = "205.174.22.25",
                    CurrencyCode = "TestCode",
                    CurrencyNumber = "111"
                },
                paymentAmt = new Domain.OrderPayment.PaymentReference.Payment
                {
                    CurrencyEquivalentPrice = new CurrencyEquivalentPrice
                    {
                        Amount = new Amount
                        {
                            MonetaryAmount = "100",
                            DecimalPrecisionCount = 2,
                            Currency = "GBP"
                        }
                    }
                },
                LoyaltyMemberDetails = new LoyaltyMember
                {
                    MemberNumber = "1234568990",
                    MemberTierCode = "Gold"
                },
                CustomerDetails = new List<Customer>
                {
                    new Customer
                    {
                        DateOfBirth = "2000-01-01",
                        Ptc = "ADT",
                        FirstName = "TestFName",
                        LastName = "TestLNAme",
                        Contacts = new Contacts
                        {
                            Email = "test@test.co.uk",
                            Phone = "9876543210",
                        }
                    }
                },
                FlightSegments = new List<Segment>
                {
                    new()
                    {
                        Legs = new List<Legs>
                        {
                            new()
                            {
                                ScheduledDepartureTime = new UtcAndLocalDateTime
                                {
                                    UtcDateTime = "2026-02-26T20:45:00Z",
                                    LocalDateTime = "2026-02-26T20:45:00+00:00"
                                },
                                ScheduledArrivalTime = new UtcAndLocalDateTime
                                {
                                    UtcDateTime = "2026-02-26T20:45:00Z",
                                    LocalDateTime = "2026-02-26T15:45:00-05:00"
                                },
                                OriginAirportCode = "LHR",
                                DestinationAirportCode = "JFK"
                            }
                        },
                        ClassOfService = "Y",
                        MarketingCarrier = "VS",
                        OperatingCarrier = "VS",
                        MarketingFlightNumber = "45",
                    }
                },
                BagsDetails =new List<Bags>
                {
                    new()
                    {
                        PassengerSequence = "1",
                        ProductDescription = "service.CBAG",
                        ProductCurrencyCode = "GBP",
                        ProductCurrencyAmt = "100",
                        ProductDecimalPrecisionCount = "0",
                        ProductCode = "CBAG",
                        ProductCategory = "SERVICE",
                        ProductItem = "2ND CHECKED BAG OR MORE 23KG"
                    }
                },
                AdditionalConfigParams= new AdditionalConfigParams
                {
                    FailureRedirectUrl = "https//testsuccess.com",
                    SuccessRedirectUrl = "https//testfailure.com",
                },
                TripType = "RETURN",
                IsBagsPayment = true
            };

            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(["DFMOBILE"]);

            var cachedChannel = new List<Channel>
            {
                new Channel
                {
                    RowKey = "DFMOBILE",
                    ChannelId = "DFMOBILE",
                    ChannelName = "channelName",
                    DlApplicationId = "dlAppId",
                    DlApplicationName = "dlApplicationName"
                }
            };

            _cachedChannelMemoryCache.Set(Constants.Application.CacheKey, cachedChannel);

            var serviceResponse = await _serviceHandler.Handle(new() { Channel = "DFMOBILE", PaymentRequest = bagsPaymentRequest }, CancellationToken.None);

            _mediator.Setup(x => x.Send(It.IsAny<OrderPaymentReferenceIdGenerateServiceCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(serviceResponse);

            var repoResponse = await _repoHandler.Handle(new() { Channel = "DFMOBILE", Request = bagsPaymentRequest }, CancellationToken.None);

            _mediator.Setup(x => x.Send(It.IsAny<OrderPaymentReferenceIdGenerateRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(repoResponse);

            //Arrange & Act
            var result = await _handler.Handle(new() { Channel = "DFMOBILE", Request = bagsPaymentRequest }, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Value.Status);
            Assert.IsType<PaymentRefIdResponse>(result.Value.Value);
            Assert.NotNull(result.Value.Value.PaymentURL);
            Assert.StartsWith("https://stg.mobilePayment", result.Value.Value.PaymentURL);
            Assert.Contains(cachedChannel[0].DlApplicationId, result.Value.Value.HPPLink);
            Assert.Contains(cachedChannel[0].RowKey, result.Value.Value.HPPLink);
            Assert.Contains("TEST_PRI_FOR_BAGGAGE_PAYMENT_TEST", result.Value.Value.HPPLink);
        }

        
    }
}
