using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Service;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.UpdateBags;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Infrastructure.Helper.Services;
using Xunit;
using ConfirmBaggagePassenger = VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage.Passenger;
using PurchaseSummaryPassengerData = VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci.PurchaseSummaryPassengerData;
using PurchaseSummaryProductData = VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci.PurchaseSummaryProductData;

namespace VAA.EAPI.Microservices.GroundOperations.Pax.Baggage.Tests
{
    public class BaggageServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public BaggageServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            BaggageTestsCommon.CreateTestEnvironmentSettings();
            _serviceProvider = new ServiceCollection()
                                .AddLogging()
                                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                .AddSingleton<IHttpConnector, HttpConnector>()
                                .AddSingleton<IAir4TokenGenerator, Air4TokenGenerator>()
                                .AddSingleton<IDeltaSsciService, DeltaSsciService>()
                                .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                .AddSingleton<IBaggageService, BaggageService>()
                                .AddSingleton<IMessageBusFactory, AzureServiceBusFactory>()
                                .AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>))
                                .AddSingleton<IEmailService, EmailService>()
                                .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()
                                .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                .BuildServiceProvider();
        }

        private IBaggageService CreateSut() => _serviceProvider.GetRequiredService<IBaggageService>();

        private (IDeltaSsciService deltaSsciService, IAzureBlobService azureBlobService, ILogger<BaggageService> logger, IEmailService emailService) GetConstructorDependencies()
        {
            return (
                _serviceProvider.GetRequiredService<IDeltaSsciService>(),
                _serviceProvider.GetRequiredService<IAzureBlobService>(),
                _serviceProvider.GetRequiredService<ILogger<BaggageService>>(),
                _serviceProvider.GetRequiredService<IEmailService>());
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBaggageServiceisNull()
        {
            // Arrange
            var (_, azureBlobService, logger, emailService) = GetConstructorDependencies();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new BaggageService(null!, azureBlobService, logger, emailService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenloggerServiceisNull()
        {
            // Arrange
            var (deltaSsciService, azureBlobService, _, emailService) = GetConstructorDependencies();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new BaggageService(deltaSsciService, azureBlobService, null!, emailService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAzureBlobServiceisNull()
        {
            // Arrange
            var (deltaSsciService, _, logger, emailService) = GetConstructorDependencies();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new BaggageService(deltaSsciService, null!, logger, emailService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenEmailServiceisNull()
        {
            // Arrange
            var (deltaSsciService, azureBlobService, logger, _) = GetConstructorDependencies();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new BaggageService(deltaSsciService, azureBlobService, logger, null!));
        }

        [Fact]
        public void ConfirmBaggagePassenger_ReturnsNull_WhenBaggageFeeAmountIsNotDecimal()
        {
            var source = new PurchaseSummaryPassengerData
            {
                ProductList =
                [
                    new PurchaseSummaryProductData
                    {
                        Description = "Baggage Fee",
                        ProductTotalAmt = "not-a-number",
                        ProductTotalAmtDecimal = "00"
                    }
                ]
            };

            var passenger = (ConfirmBaggagePassenger)source;

            Assert.Null(passenger);
        }

        [Fact]
        public void ConfirmBaggagePassenger_ParsesFractionalBaggageFeeUsingInvariantCulture()
        {
            var parsed = ConfirmBaggagePassenger.TryGetBagAmount("65", "55", out var bagAmount);

            Assert.True(parsed);
            Assert.Equal(65.55m, bagAmount);
        }

        [Fact]
        public async Task GetBaggage_ShouldThrowNullException_WhenTravelPartyIdisNull()
        {
            // Arrange
            var baggageService = CreateSut();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => baggageService.GetBaggageAllowance(null!, false));
        }

        [Fact]
        public async Task GetBaggage_ShouldReturnBaggageResponse_ForGoodRequest()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05", false);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.True(result.Value.Value.ShowBaggageAllowance);

            Assert.NotNull(result.Value.Value.Passengers);
            Assert.IsAssignableFrom<IEnumerable<PassengerAllowance>>(result.Value.Value.Passengers);
            Assert.Equal(5, result.Value.Value.Passengers.Count());
            Assert.Equal("Travel-Party-Id-Hand-05", result.Value.Value.TravelPartyId);
            Assert.Equal("https://Test.virginatlantic.com/", result.Value.Value.BaggageLearnMoreUrl);

            var firstPassenger = result.Value.Value.Passengers.First();
            Assert.Equal(("Richard", "Branson"), (firstPassenger.GivenNames, firstPassenger.Surname));
            Assert.Equal("01.01", firstPassenger.PassengerNiN);
            Assert.False(firstPassenger.IsInfantPassenger);
            Assert.NotNull(firstPassenger.BaggageOffers);
            Assert.IsAssignableFrom<IEnumerable<BaggageOffer>>(firstPassenger.BaggageOffers);
            Assert.Equal(7, firstPassenger.BaggageOffers.Count());
            Assert.Equal(0, firstPassenger.BaggageOffers.First().Price.Amount);
            Assert.Equal("3f29b5a1-4c72-4f8a-b3b1-1d2e7f6a8c9d", firstPassenger.BaggageOffers.First().BaggageOfferId);
            Assert.Equal("GBP", firstPassenger.BaggageOffers.First().Price.Currency);
            Assert.IsType<decimal>(firstPassenger.BaggageOffers.First().Price.Amount);
            Assert.IsType<decimal>(firstPassenger.BaggageOffers.First().Price.AdditionalBagPrice);
            Assert.Equal(0, firstPassenger.BaggageOffers.First().Price.Amount);
            Assert.Equal(0, firstPassenger.BaggageOffers.First().Price.AdditionalBagPrice);
            Assert.Equal(65, firstPassenger.BaggageOffers.Skip(1).First().Price.Amount);
            Assert.Equal(65, firstPassenger.BaggageOffers.Skip(1).First().Price.AdditionalBagPrice);
            Assert.Equal(205, firstPassenger.BaggageOffers.Skip(2).First().Price.Amount);
            Assert.Equal(140, firstPassenger.BaggageOffers.Skip(2).First().Price.AdditionalBagPrice);
            Assert.Equal(2, firstPassenger.NumSelectedBags);
            Assert.Null(firstPassenger.BaggageAdvisory);

            var secondPassenger = result.Value.Value.Passengers.Skip(1).First();
            Assert.Equal(0, secondPassenger.NumSelectedBags);
            Assert.Null(secondPassenger.BaggageAdvisory);

            //All About Bags
            Assert.NotNull(result.Value.Value.AllAboutBags);
            Assert.IsType<AllAboutBagsResponse>(result.Value.Value.AllAboutBags);
            Assert.IsType<List<BaggageInfo>>(result.Value.Value.AllAboutBags.EveryoneGets);
            Assert.IsType<List<BaggageInfo>>(result.Value.Value.AllAboutBags.ByCabin);
            Assert.Equal(2, result.Value.Value.AllAboutBags.EveryoneGets.Count());
            Assert.Equal(5, result.Value.Value.AllAboutBags.ByCabin.Count());
            Assert.IsType<List<string>>(result.Value.Value.AllAboutBags.EveryoneGets.First().Description);
            Assert.IsType<IconType>(result.Value.Value.AllAboutBags.EveryoneGets.First().IconType);
            Assert.Equal(IconType.carry_on, result.Value.Value.AllAboutBags.EveryoneGets.First().IconType);
            Assert.Equal(IconType.checked_bag, result.Value.Value.AllAboutBags.ByCabin.First().IconType);
            Assert.Equal("1 x 10kg carry-on bag", result.Value.Value.AllAboutBags.EveryoneGets.First().Title);
            Assert.Equal(2, result.Value.Value.AllAboutBags.EveryoneGets.First().Description.Count());
            Assert.Equal("(2 x 10kg carry-on bag in Upper Class)", result.Value.Value.AllAboutBags.EveryoneGets.First().Description.First());
            Assert.Equal("Economy Light", result.Value.Value.AllAboutBags.ByCabin.Skip(1).First().Title);
            Assert.Equal("No checked bags included", result.Value.Value.AllAboutBags.ByCabin.Skip(1).First().Description.First());

            //infant passenger 
            var infantPassenger = result.Value.Value.Passengers.Where(x => x.IsInfantPassenger).First();
            Assert.Equal("01.05", infantPassenger.PassengerNiN);
            Assert.False(infantPassenger.BaggageOffers.Any());
            Assert.NotNull(infantPassenger.BaggageAdvisory);
            Assert.IsType<BaggageAdvisory?>(infantPassenger.BaggageAdvisory);
            Assert.Equal("Infant bag allowance", infantPassenger.BaggageAdvisory.Title);
            Assert.StartsWith("Infants travelling on a lap (without their own seat)", infantPassenger.BaggageAdvisory.Message);

            //passenger with paid bag
            var paidBagPassenger = result.Value.Value.Passengers.Where(x => !x.BaggageOffers.Any(offer => offer.Price.Amount > 0) && !x.IsInfantPassenger).First();
            Assert.Equal(3, paidBagPassenger.BaggageOffers.Count());
            Assert.Equal(3, paidBagPassenger.NumSelectedBags);
            Assert.False(paidBagPassenger.CanChangeBags);
            Assert.NotNull(paidBagPassenger.BaggageAdvisory);
            Assert.StartsWith("Bags have already been added for Bagone", paidBagPassenger.BaggageAdvisory.Message);
            Assert.Contains("Bagone", paidBagPassenger.BaggageAdvisory.Message);
        }

        [Fact]
        public async Task GetBaggage_ShouldReturnShowAllowanceFalse_ForNotAllowedBaggageDuringCheckin()
        {
            //Act
            var baggageService = CreateSut();
            var result = await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05-Seamless", false);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.False(result.Value.Value.ShowBaggageAllowance);
            Assert.IsType<bool>(result.Value.Value.ShowBaggageAllowance);
        }

        [Fact]
        public async Task GetBaggage_ShouldReturnShowAllowanceFalse_ForAnyTechnicalError()
        {
            //Act
            var baggageService = CreateSut();
            var result = await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05-NotAvailable", false);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.False(result.Value.Value.ShowBaggageAllowance);
            Assert.IsType<bool>(result.Value.Value.ShowBaggageAllowance);
            Assert.Empty(result.Value.Value.Passengers);
        }

        [Fact]
        public async Task SaveBaggage_ShouldSaveBagaggeInfo_ForGoodRequest()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var baggageAllowanceResponse = (await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05", false)).Value.Value;

            var result = await baggageService.SaveBagsAsync(new SaveBaggageRequest
            {
                TravelPartyId = baggageAllowanceResponse.TravelPartyId,
                BaggagePricing =
                [ new BaggagePricing
                {
                    BaggageCount = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageCount,
                    PassengerNiN = baggageAllowanceResponse.Passengers.FirstOrDefault().PassengerNiN,
                    BaggageOfferId = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageOfferId,
                    TotalPrice = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().Price.Amount
                }
                ]
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Travel-Party-Id-Hand-05", result.Value.Value.TravelPartyId);
        }

        [Fact]
        public async Task SaveBaggage_ShouldReturnError_ForInvalidTravelPartyId()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var baggageAllowanceResponse = (await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05", false)).Value.Value;

            var result = await baggageService.SaveBagsAsync(new SaveBaggageRequest
            {
                TravelPartyId = "Invalid-Travel-Party-Id",
                BaggagePricing =
                [ new BaggagePricing
                {
                    BaggageCount = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageCount,
                    PassengerNiN = baggageAllowanceResponse.Passengers.FirstOrDefault().PassengerNiN,
                    BaggageOfferId = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageOfferId,
                    TotalPrice = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().Price.Amount
                }
                ]
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Value);
            Assert.Equal(Status.Error, result.Value.Status);
        }

        [Fact]
        public async Task CheckPayment_ShouldUpdatePaymentStatus_ForFreeBags()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var baggageAllowanceResponse = (await baggageService.GetBaggageAllowance("Travel-Party-Id-Hand-05", false)).Value.Value;

            var saveBaggageResponse = await baggageService.SaveBagsAsync(new SaveBaggageRequest
            {
                TravelPartyId = baggageAllowanceResponse.TravelPartyId,
                BaggagePricing =
                [ new BaggagePricing
                {
                    BaggageCount = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageCount,
                    PassengerNiN = baggageAllowanceResponse.Passengers.FirstOrDefault().PassengerNiN,
                    BaggageOfferId = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().BaggageOfferId,
                    TotalPrice = baggageAllowanceResponse.Passengers.FirstOrDefault().BaggageOffers.FirstOrDefault().Price.Amount
                }
                ]
            });

            var result = await baggageService.CheckPaymentAsync(new CheckPaymentRequest
            {
                TravelPartyId = saveBaggageResponse.Value.Value.TravelPartyId,
                PaymentPending = false
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Travel-Party-Id-Hand-05", result.Value.Value.TravelPartyId);
        }

        [Fact]
        public async Task CheckPayment_ShouldReturnError_ForInvalidTravelPartyId()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.CheckPaymentAsync(new CheckPaymentRequest
            {
                TravelPartyId = "Invalid-Travel-Party-Id_1",
                PaymentPending = false
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Value);
            Assert.Equal(Status.Error, result.Value.Status);
        }

        [Fact]
        public async Task ConfirmBag_ShouldReturnError_ForInvalidTravelPartyId()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Invalid-Travel-Party-Id_1",
                PaymentReferenceId = "InvalidPaymentRefId"
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Value);
            Assert.Equal(Status.Error, result.Value.Status);
        }

        [Fact]
        public async Task ConfirmBag_ShouldReturnErrorMessage_AndDeveloperMessage_WhenPaymentFailed()
        {
            //Act
            var baggageService = CreateSut();

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Hand-08",
                PaymentReferenceId = "TestPaymentRefId123"
            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Value);
            Assert.Equal(Status.Error, result.Value.Status);
            Assert.NotNull(result.Value.Errors);

            var error = result.Value.Errors.First();
            Assert.Equal("Baggage payment failed", error.Title);
            Assert.Equal("Baggage payment failed", error.Message);
            Assert.Equal("Unable to process bag payment. Please try again or contact an agent.", error.DeveloperMessage);
        }

        [Fact]
        public async Task ConfirmBaggage_ForGoodRequest()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Hand-05",
                CardHolderDetail = new Passenger.Baggage.Domain.ConfirmBaggage.CardHolderDetail
                {
                    CardType = "Visa",
                    CardNumber = "4111111111111111",
                    CardHolderFirstName = "Test",
                    CardHolderLastName = "User",
                    ExpiryMonth = "12",
                    ExpiryYear = "2026",
                    CardVerificationValue = "123"
                },
                BillingAddress = new Passenger.Baggage.Domain.ConfirmBaggage.BillingAddress
                {
                    AddressLine1 = "123 Test St",
                    AddressLine2 = "Apt 4",
                    City = "Testville",
                    Country = "UK",
                    PostalCode = "TE57 1NG"
                },

            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("LHR", result.Value.Value.OriginAirportCode);
            Assert.Equal("JFK", result.Value.Value.DestinationAirportCode);
            Assert.Equal("GBP", result.Value.Value.CurrencyCode);
            Assert.Equal("Yellow", result.Value.Value.PassengerList.FirstOrDefault().FirstName);
            Assert.Equal("Bandit", result.Value.Value.PassengerList.FirstOrDefault().LastName);
        }


        [Fact]
        public async Task ConfirmBaggage_ForGoodRequest_MultiPax()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Hand-06",
                CardHolderDetail = new Passenger.Baggage.Domain.ConfirmBaggage.CardHolderDetail
                {
                    CardType = "Visa",
                    CardNumber = "4111111111111111",
                    CardHolderFirstName = "Test",
                    CardHolderLastName = "User",
                    ExpiryMonth = "12",
                    ExpiryYear = "2026",
                    CardVerificationValue = "123"
                },
                BillingAddress = new Passenger.Baggage.Domain.ConfirmBaggage.BillingAddress
                {
                    AddressLine1 = "123 Test St",
                    AddressLine2 = "Apt 4",
                    City = "Testville",
                    Country = "UK",
                    PostalCode = "TE57 1NG"
                },

            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("LHR", result.Value.Value.OriginAirportCode);
            Assert.Equal("ATL", result.Value.Value.DestinationAirportCode);
            Assert.Equal("GBP", result.Value.Value.CurrencyCode);
            Assert.True(result.Value.Value.PassengerList.Count() > 1);
            Assert.Equal("335.00", result.Value.Value.TotalPaidAmount);
            //pasenger 1
            var passenger1 = result.Value.Value.PassengerList.FirstOrDefault();
            Assert.Equal("Lashaun", passenger1.FirstName);
            Assert.Equal("Raffield", passenger1.LastName);
            Assert.Equal("GBP", passenger1.CurrencyCode);
            Assert.Equal(65, passenger1.BagAmount);
        }

        [Fact]
        public async Task ConfirmBaggage_ForGoodRequest_XCDCurrency()
        {
            //Act
            var baggageService = _serviceProvider.GetService<IBaggageService>()!;

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Hand-07",
                CardHolderDetail = new Passenger.Baggage.Domain.ConfirmBaggage.CardHolderDetail
                {
                    CardType = "Visa",
                    CardNumber = "4111111111111111",
                    CardHolderFirstName = "Test",
                    CardHolderLastName = "User",
                    ExpiryMonth = "12",
                    ExpiryYear = "2026",
                    CardVerificationValue = "123"
                },
                BillingAddress = new Passenger.Baggage.Domain.ConfirmBaggage.BillingAddress
                {
                    AddressLine1 = "123 Test St",
                    AddressLine2 = "Apt 4",
                    City = "Testville",
                    Country = "UK",
                    PostalCode = "TE57 1NG"
                },

            });

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("ANU", result.Value.Value.OriginAirportCode);
            Assert.Equal("LHR", result.Value.Value.DestinationAirportCode);
            Assert.Equal("XCD", result.Value.Value.CurrencyCode);
            Assert.Equal("XCD", result.Value.Value.CurrencySymbol);
            Assert.Equal("746.48", result.Value.Value.TotalPaidAmount);
            Assert.Single(result.Value.Value.PassengerList);
        }

        [Fact]
        public async Task ConfirmBaggage_UsesCurrencyCode_WhenTheDownstreamReceiptDoesNotProvideASymbol()
        {
            var baggageService = CreateSut();

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-ZAR"
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("ZAR", result.Value.Value.CurrencyCode);
            Assert.Equal("ZAR", result.Value.Value.CurrencySymbol);
        }

        [Fact]
        public async Task ConfirmBaggage_ReturnsError_WhenTheSuccessfulReceiptContainsNoBaggageFeeProduct()
        {
            var baggageService = CreateSut();

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Empty-Receipt"
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.Equal("Downstream receipt did not contain valid baggage fee products.", result.Value.Errors.First().DeveloperMessage);
        }

        [Fact]
        public async Task ConfirmBaggage_ReturnsError_WhenTheReceiptContainsAnInvalidBaggageFeeAmount()
        {
            var baggageService = CreateSut();

            var result = await baggageService.ConfirmBaggageAsync(new Passenger.Baggage.Domain.ConfirmBaggage.ConfirmBaggageRequest
            {
                TravelPartyId = "Travel-Party-Id-Invalid-Amount"
            });

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Null(result.Value.Value);
            Assert.Equal("Downstream receipt did not contain valid baggage fee products.", result.Value.Errors.First().DeveloperMessage);
        }


    }
}
