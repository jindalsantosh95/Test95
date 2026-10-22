using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink;
using VAA.EAPI.Microservices.Retail.Trip.Domain.Enum;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class DeeplinkServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;


        public DeeplinkServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            TripMSEnvironmentSetup.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var deltaEncryptionService = new Mock<IDeltaEncryptionService>();
            var deltaEncryptionPrivateService = new Mock<IDeltaEncryptionPrivateService>();

            _serviceProvider ??= new ServiceCollection().AddLogging()
                                                        .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                        .AddSingleton<IHttpConnector, HttpConnector>()
                                                        .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                        .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                        .AddSingleton<IBidForUpgradeOffersService, BidForUpgradeOffersService>()
                                                        .AddSingleton<IDeltaEncryptionService>(deltaEncryptionService.Object)
                                                        .AddSingleton<IDeltaEncryptionPrivateService>(deltaEncryptionPrivateService.Object)
                                                        .AddSingleton<IDeeplinkService, DeeplinkService>()
                                                        .BuildServiceProvider();
        }

        private static string CreateTripReservationId(
            string? bookingId = null,
            string? givenName = null,
            string? surname = null,
            string? pnrLocator = null,
            string? dob = null,
            string? basketId = null)
        {
            var claims = new List<Claim>();

            if (bookingId is not null) claims.Add(new Claim("msBookingId", bookingId));
            if (givenName is not null) claims.Add(new Claim("givenName", givenName));
            if (surname is not null) claims.Add(new Claim("surName", surname));
            if (pnrLocator is not null) claims.Add(new Claim("pnrLocator", pnrLocator));
            if (dob is not null) claims.Add(new Claim("dob", dob));
            if (basketId is not null) claims.Add(new Claim("basketId", basketId));

            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now.AddMinutes(-1),
                Expires = now.AddHours(1),
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigReader.Get(VAA.EAPI.Microservices.Retail.Trip.Constant.Environment.BookingIdJwtKey))),
                    SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForGBPOS()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6ImZkYmQxYTY5LTY5ZmUtNGU3Ny1hMTNmLTYxNTliZWQ0ZDI4MiIsImdpdmVuTmFtZSI6IkZFUkRJTkFORCIsInN1ck5hbWUiOiJCVVRUUkFZIiwicG5yTG9jYXRvciI6IkUyNUc2NCIsImJhc2tldElkIjoiM2VhNDI1NTAtZGRhNC00NjJiLTllMzEtZTBhZWM5NTMxYWZjIiwibmJmIjoxNzYxMzI0NDY2LCJleHAiOjE3NjE0MTA4NjYsImlhdCI6MTc2MTMyNDQ2Nn0.rQVnpK285Pr4F7l6Vwr1VC-IejiwVOMagqi8RMs-rwg",
                DeeplinkOption = DeeplinkOption.TravelInsurance,
                POSCountryCode = "GB"
            };

            //Act
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.True(response.IsSuccess == true);
            Assert.Equal("https://www.magroup-online.com/WL/VAA/GB/en", response.Value.Value.DeeplinkUrl);
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForUSPOS()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "2b724508-7a8f-4e68-b1dc-bcebd7cf174f",
                DeeplinkOption = DeeplinkOption.TravelInsurance,
                POSCountryCode = "US"
            };

            //Act
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.True(response.IsSuccess == true);
            Assert.Equal("https://www.etravelprotection.com/virginatlantic/home", response.Value.Value.DeeplinkUrl);
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForINPOS()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "2b724508-7a8f-4e68-b1dc-bcebd7cf174f",
                DeeplinkOption = DeeplinkOption.TravelInsurance,
                POSCountryCode = "IN"
            };

            //Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => deeplinkService.GetDeeplinkUrlDetails(payload));
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForIROPViewDetails()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6ImUzMWQ1Mjk1LTYyMWQtNDAxYi1hOTY1LTcyYWIxYWJhY2JiZSIsImdpdmVuTmFtZSI6Ik1BUlZJTiIsInN1ck5hbWUiOiJNQ0NPTExFWSIsInBuckxvY2F0b3IiOiJFTkc0UVkiLCJiYXNrZXRJZCI6ImUzM2ExOWZjLTgzNjktNDRhYy05YmM0LWEwNzUxNWE3MTQ3MCIsIm5iZiI6MTc2MTU3MTUwNCwiZXhwIjo0OTE3MjQ1MTA0LCJpYXQiOjE3NjE1NzE1MDR9.A4VhQ4bt9ooJuBHY4wi8Lj-rTL1c8i9K8zW_JkZszJo",
                DeeplinkOption = DeeplinkOption.IROP_ViewDetails
            };

            //Act 
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.NotNull(response.Value.Value);

            var cacheKey = response.Value.Value.DeeplinkUrl.Split('=')[1];
            var cachedData = await distributedCacheService.Get<DeeplinkEndpointDetails>(cacheKey);

            Assert.True(response.IsSuccess == true);
            Assert.NotNull(response.Value.Value.DeeplinkUrl);
            Assert.Matches(@"^https:\/\/proxydeeplinktest\.com\/deeplink\?id=.*", response.Value.Value.DeeplinkUrl);
            Assert.True(response.Value.Value.DeeplinkOption == DeeplinkOption.IROP_ViewDetails);
            Assert.NotNull(cachedData);
        }

        [Fact]
        public async Task GetDeeplinkPostDetailsForProxy_Fails()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var strCacheId = "invalidCacheId";

            //Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => deeplinkService.GetDeeplinkDetailsForProxy(strCacheId));
        }

        [Fact]
        public async Task GetDeeplinkPostDetailsForProxy_CachedValue_ReturnsSuccess()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6ImUzMWQ1Mjk1LTYyMWQtNDAxYi1hOTY1LTcyYWIxYWJhY2JiZSIsImdpdmVuTmFtZSI6Ik1BUlZJTiIsInN1ck5hbWUiOiJNQ0NPTExFWSIsInBuckxvY2F0b3IiOiJFTkc0UVkiLCJiYXNrZXRJZCI6ImUzM2ExOWZjLTgzNjktNDRhYy05YmM0LWEwNzUxNWE3MTQ3MCIsIm5iZiI6MTc2MTU3MTUwNCwiZXhwIjo0OTE3MjQ1MTA0LCJpYXQiOjE3NjE1NzE1MDR9.A4VhQ4bt9ooJuBHY4wi8Lj-rTL1c8i9K8zW_JkZszJo",
                DeeplinkOption = DeeplinkOption.IROP_ViewDetails
            };
            var responseCached = await deeplinkService.GetDeeplinkUrlDetails(payload);
            var strCacheId = responseCached.Value.Value.DeeplinkUrl.Split('=')[1]; ;

            //Act 
            var response = await deeplinkService.GetDeeplinkDetailsForProxy(strCacheId);

            //Assert
            Assert.NotNull(response.Value.Value);
            Assert.NotNull(response.Value.Value.Endpoint);
            Assert.Equal("https://testdeeplink.com/irop/details", response.Value.Value.Endpoint?.EndpointUrl);
            Assert.NotNull(response.Value.Value.Parameters);
            Assert.True(response.Value.Value.Parameters.Any());
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForIROPFAF_ReturnSuccess()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6IjA2OTYwMjM3LTY2MmEtNDJmMi1iYmYyLTExMGMyNDVlMTA3MCIsImdpdmVuTmFtZSI6Im9uZSIsInN1ck5hbWUiOiJ2aXJnaW4iLCJwbnJMb2NhdG9yIjoiRVY2RjM2IiwiYmFza2V0SWQiOiJiZjRiNTczYS1lZjMyLTRmNWUtOWY2Yy04Nzk3NzM5Mjc5NmYiLCJuYmYiOjE3NjIyOTU3NDEsImV4cCI6NDkxNzk2OTM0MSwiaWF0IjoxNzYyMjk1NzQxfQ.yPmh35QHkoK8l518RkPpM2M1-CL2ST1AWOhhvxyEflo",
                DeeplinkOption = DeeplinkOption.IROP_FAF
            };

            //Act 
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.NotNull(response.Value.Value);

            var cacheKey = response.Value.Value.DeeplinkUrl.Split('=')[1];
            var cachedData = await distributedCacheService.Get<DeeplinkEndpointDetails>(cacheKey);

            Assert.True(response.IsSuccess == true);
            Assert.NotNull(response.Value.Value.DeeplinkUrl);
            Assert.Matches(@"^https:\/\/proxydeeplinktest\.com\/deeplink\?id=.*", response.Value.Value.DeeplinkUrl);
            Assert.True(response.Value.Value.DeeplinkOption == DeeplinkOption.IROP_FAF);
            Assert.NotNull(cachedData);
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ForPOPSMeal_ReturnSuccess()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6IjA2OTYwMjM3LTY2MmEtNDJmMi1iYmYyLTExMGMyNDVlMTA3MCIsImdpdmVuTmFtZSI6Im9uZSIsInN1ck5hbWUiOiJ2aXJnaW4iLCJwbnJMb2NhdG9yIjoiRVY2RjM2IiwiYmFza2V0SWQiOiJiZjRiNTczYS1lZjMyLTRmNWUtOWY2Yy04Nzk3NzM5Mjc5NmYiLCJuYmYiOjE3NjIyOTU3NDEsImV4cCI6NDkxNzk2OTM0MSwiaWF0IjoxNzYyMjk1NzQxfQ.yPmh35QHkoK8l518RkPpM2M1-CL2ST1AWOhhvxyEflo",
                DeeplinkOption = DeeplinkOption.POPSMeal,
                FlightNumber = "411"
            };

            //Act 
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.NotNull(response.Value.Value);
            Assert.True(response.IsSuccess == true);
            Assert.NotNull(response.Value.Value.DeeplinkUrl);
            Assert.Matches(@"^https:\/\/popsdeeplinktest\.com\/en\.html\?token=.*", response.Value.Value.DeeplinkUrl);
            Assert.True(response.Value.Value.DeeplinkOption == DeeplinkOption.POPSMeal);

        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ManageHoliday_ReturnSuccess()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = CreateTripReservationId(
                    bookingId: "6837647",
                    surname: "Johnson",
                    dob: "1970-04-15"),
                DeeplinkOption = DeeplinkOption.ManageHoliday                
            };

            //Act 
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            //Assert
            Assert.NotNull(response.Value.Value);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Value.Value.DeeplinkUrl);
            Assert.Matches(@"^https:\/\/proxydeeplinktest\.com\/deeplink\?id=.*", response.Value.Value.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.ManageHoliday, response.Value.Value.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ConciergeTripAware_WithHolidayBooking_CachesHolidayItem()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = CreateTripReservationId(
                    bookingId: "6837647",
                    surname: "Johnson",
                    dob: "1970-04-15"),
                DeeplinkOption = DeeplinkOption.ConciergeTripAware
            };

            // Act
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            // Assert
            Assert.NotNull(response.Value.Value);
            var deeplinkUrl = Assert.IsType<string>(response.Value.Value.DeeplinkUrl);
            Assert.Matches(@"^https:\/\/proxydeeplinktest\.com\/deeplink\?id=.*", deeplinkUrl);

            var deeplinkUri = new Uri(deeplinkUrl);
            var cacheKey = deeplinkUri.Query.Replace("?id=", string.Empty);
            var cachedData = await distributedCacheService.Get<DeeplinkEndpointDetails>(cacheKey);
            var items = cachedData!.Items.Select(item => Assert.IsType<JObject>((object)item!)).ToList();

            Assert.True(response.IsSuccess);
            Assert.Equal(DeeplinkOption.ConciergeTripAware, response.Value.Value.DeeplinkOption);
            Assert.NotNull(cachedData);
            Assert.Single(items);
            Assert.Equal("holiday", items[0]["type"]?.Value<string>());
            Assert.Equal("6837647", items[0]["bookingReference"]?.Value<string>());
            Assert.Equal("Johnson", items[0]["surname"]?.Value<string>());
            Assert.Equal("1970-04-15", items[0]["dob"]?.Value<string>());
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ConciergeTripAware_WithCommaSeparatedTripReservationIds_CachesEachItem()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();
            var holidayTripReservationId = CreateTripReservationId(
                bookingId: "6837647",
                surname: "Johnson",
                dob: "1970-04-15");
            var flightTripReservationId = CreateTripReservationId(
                givenName: "Jane",
                surname: "Smith",
                pnrLocator: "ABC123");
            var commaSeparatedTripReservationIds = string.Join(",", holidayTripReservationId, flightTripReservationId);
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = commaSeparatedTripReservationIds,
                DeeplinkOption = DeeplinkOption.ConciergeTripAware
            };

            // Act
            var response = await deeplinkService.GetDeeplinkUrlDetails(payload);

            // Assert
            Assert.Matches(@"^https:\/\/proxydeeplinktest\.com\/deeplink\?id=.*", response.Value.Value.DeeplinkUrl);
            var deeplinkUrl = Assert.IsType<string>(response.Value.Value.DeeplinkUrl);
            var deeplinkUri = new Uri(deeplinkUrl);
            var cacheKey = deeplinkUri.Query.Replace("?id=", string.Empty);
            var cachedData = await distributedCacheService.Get<DeeplinkEndpointDetails>(cacheKey);
            var items = cachedData!.Items.Select(item => Assert.IsType<JObject>((object)item!)).ToList();

            Assert.True(response.IsSuccess);
            Assert.Equal(DeeplinkOption.ConciergeTripAware, response.Value.Value.DeeplinkOption);
            Assert.NotNull(cachedData);
            Assert.Equal(2, items.Count);
            Assert.Equal("holiday", items[0]["type"]?.Value<string>());
            Assert.Equal("6837647", items[0]["bookingReference"]?.Value<string>());
            Assert.Equal("Johnson", items[0]["surname"]?.Value<string>());
            Assert.Equal("1970-04-15", items[0]["dob"]?.Value<string>());
            Assert.Equal("flight", items[1]["type"]?.Value<string>());
            Assert.Equal("ABC123", items[1]["pnrLocator"]?.Value<string>());
            Assert.Equal("Jane", items[1]["givenName"]?.Value<string>());
            Assert.Equal("Smith", items[1]["surname"]?.Value<string>());
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ConciergeTripAware_WithIncompleteClaims_ThrowsArgumentException()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {
                TripReservationId = CreateTripReservationId(bookingId: "6837647"),
                DeeplinkOption = DeeplinkOption.ConciergeTripAware
            };

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => deeplinkService.GetDeeplinkUrlDetails(payload));
        }

        [Fact]
        public async Task GetDeeplinkUrlDetails_ManageHoliday_WithoutTripReservationId_ThrowsException()
        {
            // Arrange
            var deeplinkService = _serviceProvider.GetRequiredService<IDeeplinkService>();
            var payload = new DeepLinkRequestPayload()
            {                
                DeeplinkOption = DeeplinkOption.ManageHoliday
            };

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await deeplinkService.GetDeeplinkUrlDetails(payload));
        }
    }
}
