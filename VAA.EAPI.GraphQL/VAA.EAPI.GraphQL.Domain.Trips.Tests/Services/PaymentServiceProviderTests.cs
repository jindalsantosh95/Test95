using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services
{
    public class PaymentServiceProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly PaymentServiceProvider _provider;
        private readonly ILogger<HttpConnector> _logger;

        public PaymentServiceProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _provider = new PaymentServiceProvider(_httpConnector); 
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task PaymentReferenceId_ReturnsExpectedResult()
        {
            // Arrange
            var paymentRequest = new PaymentRequestMs()
            { 
                Pos = new() { 
                    CityCode = "LON", CountryCode = "GB", CurrencyCode = "GBP", IpAddress = "85.115.53.202" 
                } ,
                PaymentAmt = new() { 
                    CurrencyEquivalentPrice = new() { Amount = new() 
                    { MonetaryAmount = "0", Currency = "", DecimalPrecisionCount = 0 } 
                    }
                },
                CustomerDetails = new List<CustomerInfo> { new() { FirstName = "TESTFNAME", LastName = "TESTLNAME", Ptc = "ADT" } },
                 FlightSegments = new List<Segment>
                {
                    new() {
                        SegmentId = "1",
                        MarketingFlightNumber = "003",
                        MarketingCarrier = "VS",
                        OperatingFlightNumber = "003",
                        OperatingCarrier = "VS",
                        OriginAirportCode = "LHR",
                        DestinationAirportCode = "JFK",
                        FareBasis = "BY9QBDSA",
                        ClassOfService = "Y",
                        ScheduledArrivalTime = new()
                        {
                            LocalDateTime = "2025-09-20T12:45"
                        },
                        ScheduledDepartureTime = new()
                        {
                            LocalDateTime = "2025-09-20T09:35"
                        },
                        Legs = new List<Leg>
                        {
                            new()
                            {
                                ScheduledDepartureTime = new()
                                {
                                    UtcDateTime = null,
                                    LocalDateTime = "2025-09-20T09:35"
                                },
                                OriginAirportCode = "LHR",
                                ScheduledArrivalTime = new()
                                {
                                    UtcDateTime = null,
                                    LocalDateTime = "2024-09-20T12:45"
                                },
                                DestinationAirportCode = "JFK"
                            }
                        }
                    }
                },
                SeatDetails = new List<Seat>
                {
                    new Seat
                    {
                        PassengerSequence = "1",
                        TripId = "1",
                        TripSeq = "1",
                        AssignedSeatNumber = null,
                        PendingSeatNumber = "70H",
                        PendingSeatPriceAmount = "24.00",
                        PendingSeatDecimalPrecisionCount = "2",
                        PendingSeatCurrencyCode = "GBP",
                        SeatOfferDetails = new SeatOfferDetails
                        {
                            DeltaSeatOfferId = "c4c36d3c-9294-4e7f-8e0f-2cdb7b77c3b5_FA",
                            BrandDescription = "Economic Classic",
                            ProductDescription = "Standard Seat - Economy Cabin"
                        }
                    }
                },
                AdditionalConfigParams = new AdditionalConfigParams
                {
                    FailureRedirectUrl = "",
                    MultiProductFlag = true,
                    SessionID = null,
                    SuccessRedirectUrl = "",
                    IsInsuranceAdded = false
                },
                TripType = "MYB"
            };
            var paymentRequestJson = Json.Serialize(paymentRequest);
            var expectedResponse = new APIResponse<PaymentRefIdResponseMs>
            {
                Response = new PaymentRefIdResponseMs
                {
                    HPPLink = "https://enterprisepaymentshpp-si.vs01.vs.air4.com/hpp?paymentReferenceId=MBDTGOEI6973VPXNCJRHSZW1K45UL0YF&channelId=DFWEB&appId=DC&purchasePath=DFWEBBOOKING"
                }
            };

            // Act
            var result = await _provider.PaymentReferenceId(paymentRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Response.HPPLink, result.HPPLink);
            Assert.Contains("https://enterprisepaymentshpp-si.vs01.vs.air4.com/hpp?paymentReferenceId=", result.HPPLink);
        }

        [Fact]
        public async Task PaymentReferenceId_ThrowsException_OnNullRequest()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.PaymentReferenceId(null));
        }

        [Fact]
        public async Task HolidayPaymentAsync_ReturnsExpectedResult()
        {
            Setup.CreateTestEnvironmentSettings();

            var result = await _provider.HolidayPaymentAsync(new HolidayPaymentRequestMs
            {
                BookingReference = "booking-123",               
                Surname = "Doe",
                DateOfBirth = "1990-01-01",
                AmountToPay = 2758.65m,
                ReturnUrl = "r",
                ExpiryUrl = "e",
                PayPalClientUrl = "p",
                ContextPath = "c"
            });

            Assert.NotNull(result);
            Assert.Equal(2758.65m, result.AmountToPay);
            Assert.Equal("PENDING", result.Status);
            Assert.Equal("https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment/hpp?id=E0f_TGuAYxjDZ-WuF6bT8Q", result.PaymentLink);
            Assert.Equal("https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment", result.RemoveLink);
            Assert.NotNull(result.PaymentAmount);
            Assert.Equal(2758.65m, result.PaymentAmount.AmountToPay);
            Assert.Equal("https://stg.virginatlantic.com/cjs-checkout-api/v2/cpd/payment/amount", result.PaymentAmount.Link);
        }

        [Fact]
        public async Task HolidayPaymentAsync_ThrowsException_OnNullRequest()
        {
            Setup.CreateTestEnvironmentSettings();
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.HolidayPaymentAsync(null));
        }
    }
}
