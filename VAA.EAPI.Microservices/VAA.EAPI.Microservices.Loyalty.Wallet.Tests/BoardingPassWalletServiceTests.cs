using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using VAA.EAPI.Common.Extensions;
using TestDistributedCache = VAA.EAPI.Core.Xunit.Caching.DistributedCache;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Service;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class BoardingPassWalletServiceTests : IClassFixture<HttpClientFactoryFixture>, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TestDistributedCache _distributedCache;
        public BoardingPassWalletServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _distributedCache = new TestDistributedCache();
            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IDistributedCache>(_ => _distributedCache)
                .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                .AddSingleton<IAirshipWalletService, AirshipWalletService>()
                .AddSingleton<IBoardingPassWalletService, BoardingPassWalletService>()
                .AddLogging()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _distributedCache.Dispose();
            _serviceProvider.Dispose();
        }

        [Fact]
        public void Constructor_ShouldThrowError_WhenLoggerIsNull()
        {
            var airshipWalletService = _serviceProvider.GetRequiredService<IAirshipWalletService>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();

            Assert.Throws<ArgumentNullException>(() =>
                new BoardingPassWalletService(null, airshipWalletService, distributedCacheService));
        }

        [Fact]
        public void Constructor_ShouldThrowError_WhenAirshipWalletServiceIsNull()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<BoardingPassWalletService>>();
            var distributedCacheService = _serviceProvider.GetRequiredService<IDistributedCacheService>();

            Assert.Throws<ArgumentNullException>(() =>
                new BoardingPassWalletService(logger, null, distributedCacheService));
        }

        [Fact]
        public void Constructor_ShouldThrowError_WhenDistributedCacheServiceIsNull()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<BoardingPassWalletService>>();
            var airshipWalletService = _serviceProvider.GetRequiredService<IAirshipWalletService>();

            Assert.Throws<ArgumentNullException>(() =>
                new BoardingPassWalletService(logger, airshipWalletService, null));
        }

        [Fact]
        public async Task Service_ShouldThrowArgumentNullException_ForNullRequest()
        {
            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateWalletLinksAsync(null));
        }

        [Fact]
        public async Task Service_ShouldThrowArgumentNullException_ForNullAirportDetailsinRequest()
        {
            var request = CreateWalletPassRequest();
            request.Airports = null;

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateWalletLinksAsync(request));
        }

        [Fact]
        public async Task Service_ShouldThrowArgumentNullException_ForNullPassengerDetailsinRequest()
        {
            var request = CreateWalletPassRequest();
            request.Passengers = null;

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateWalletLinksAsync(request));
        }

        [Fact]
        public async Task Service_ShouldThrowArgumentNullException_ForNullBoardingPassesinRequest()
        {
            var request = CreateWalletPassRequest();
            request.BoardingPasses = null;

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateWalletLinksAsync(request));
        }

        [Fact]
        public async Task Service_ShouldThrowArgumentNullException_ForNullSegmentDetailsinRequest()
        {
            var request = CreateWalletPassRequest();
            request.Segments = null;

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateWalletLinksAsync(request));
        }

        [Fact]
        public async Task Service_ShouldReturnWalletLinks_ForGoodRequest()
        {
            var request = CreateWalletPassRequest();

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            var result = await provider.GenerateWalletLinksAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
            Assert.True(result.Value.Value.WalletPasses.Any());

            var passes = result.Value.Value.WalletPasses;

            Assert.Equal(6, passes.Count());    // 2 segment slice (JFK - LHR - DEL) with 3 passengers
            Assert.IsType<List<WalletPassResponse>>(passes);
            Assert.Equal(3, passes.Where(x => x.OriginAirportCode.Is(request.Segments.First().Origin.Code)).Count());
            Assert.Equal(3, passes.Where(x => x.OriginAirportCode.Is(request.Segments.Skip(1).First().Origin.Code)).Count());
            Assert.All(passes, x => Assert.NotNull(x.BoardingPassAdaptiveLink));
            Assert.All(passes, x => Assert.NotNull(x.IosBoardingPassWalletLink));
            Assert.All(passes, x => Assert.NotNull(x.AndroidBoardingPassWalletLink));

            Assert.False(result.Value.Value.PnrMultiBoardingPassUrl.IsNullOrWhiteSpace());

            var pnrMultiUrl = result.Value.Value.PnrMultiBoardingPassUrl;
            var pnrMultiUri = new Uri(pnrMultiUrl);
            Assert.Equal("/v1/pass/adaptive", pnrMultiUri.AbsolutePath);
            Assert.Null(GetQueryValue(pnrMultiUri, "deviceType"));

            var pnrIds = GetQueryValue(pnrMultiUri, "ids");
            Assert.False(pnrIds.IsNullOrWhiteSpace());
            Assert.InRange(pnrIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Length, 1, 10);

            var pnrMultiUrlIos = result.Value.Value.IosPnrMultiBoardingPassUrl;
            var pnrMultiUrlAndroid = result.Value.Value.AndroidPnrMultiBoardingPassUrl;
            Assert.False(pnrMultiUrlIos.IsNullOrWhiteSpace());
            Assert.False(pnrMultiUrlAndroid.IsNullOrWhiteSpace());

            var pnrMultiUriIos = new Uri(pnrMultiUrlIos);
            var pnrMultiUriAndroid = new Uri(pnrMultiUrlAndroid);
            Assert.Equal(pnrMultiUri.AbsolutePath, pnrMultiUriIos.AbsolutePath);
            Assert.Equal(pnrMultiUri.AbsolutePath, pnrMultiUriAndroid.AbsolutePath);
            Assert.Equal(pnrIds, GetQueryValue(pnrMultiUriIos, "ids"));
            Assert.Equal(pnrIds, GetQueryValue(pnrMultiUriAndroid, "ids"));
            Assert.Equal("ios", GetQueryValue(pnrMultiUriIos, "deviceType"));
            Assert.Equal("android", GetQueryValue(pnrMultiUriAndroid, "deviceType"));

            foreach (var passengerNin in request.Passengers.Select(p => $"{p.PassengerFirstNIN}.{p.PassengerLastNIN}"))
            {
                var passengerResponses = passes.Where(x => x.PassengerNIN.Is(passengerNin)).ToList();
                Assert.NotEmpty(passengerResponses);
                Assert.Single(passengerResponses.Select(x => x.BoardingPassAdaptiveLink).Distinct());
                Assert.Single(passengerResponses.Select(x => x.IosBoardingPassWalletLink).Distinct());
                Assert.Single(passengerResponses.Select(x => x.AndroidBoardingPassWalletLink).Distinct());
            }

            //First Passenger
            var psgOne = passes.Where(x => x.PassengerNIN.Is("01.01"));
            Assert.Equal(2, psgOne.Count());
            Assert.Equal("JFK", psgOne.MinBy(x => x.DepartureDateLocalDateTime)!.OriginAirportCode);
            Assert.IsType<string>(psgOne.First().BoardingPassAdaptiveLink);
            Assert.Equal("ios", GetQueryValue(new Uri(psgOne.First().IosBoardingPassWalletLink), "deviceType"));
            Assert.Equal("android", GetQueryValue(new Uri(psgOne.First().AndroidBoardingPassWalletLink), "deviceType"));
        }

        [Fact]
        public async Task Service_NormalizesCarrierCode_AndMergesLinksFromMultipleTemplateRequests()
        {
            var request = CreateWalletPassRequest();
            request.Segments.First().OperatingCarrier.CarrierCode = " VS ";
            request.BoardingPasses.First().CabinName = "Upper";
            request.BoardingPasses.First().TemplateCode = TemplateCode.VS_UPPER;

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            var result = await provider.GenerateWalletLinksAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(request.BoardingPasses.Count(), result.Value.Value.WalletPasses.Count());
            Assert.All(result.Value.Value.WalletPasses, pass => Assert.False(pass.BoardingPassAdaptiveLink.IsNullOrWhiteSpace()));
            Assert.Equal("https://wallet-api.asnapieu.com/v1/pass/adaptive?ids=upper-link,BbVr7E1KZmt", result.Value.Value.WalletPasses.First(pass => pass.PassengerNIN.Is("01.01") && pass.OriginAirportCode.Is("JFK")).BoardingPassAdaptiveLink);
        }

        [Fact]
        public async Task Service_SinglePassengerMultiSegment_ReturnsNullPnrMultiBoardingPassUrls()
        {
            var request = CreateWalletPassRequest();
            request.Passengers = request.Passengers.Take(1).ToList();

            var firstPassenger = request.Passengers.First();
            var passengerNin = $"{firstPassenger.PassengerFirstNIN}.{firstPassenger.PassengerLastNIN}";
            request.BoardingPasses = request.BoardingPasses.Where(x => x.PassengerNIN.Is(passengerNin)).ToList();

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            var result = await provider.GenerateWalletLinksAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);

            Assert.Equal(2, result.Value.Value.WalletPasses.Count()); // 2 segment slice (JFK - LHR - DEL) for 1 passenger
            Assert.All(result.Value.Value.WalletPasses, x => Assert.NotNull(x.BoardingPassAdaptiveLink));
            Assert.All(result.Value.Value.WalletPasses, x => Assert.NotNull(x.IosBoardingPassWalletLink));
            Assert.All(result.Value.Value.WalletPasses, x => Assert.NotNull(x.AndroidBoardingPassWalletLink));

            Assert.Single(result.Value.Value.WalletPasses.Select(x => x.BoardingPassAdaptiveLink).Distinct());
            Assert.Single(result.Value.Value.WalletPasses.Select(x => x.IosBoardingPassWalletLink).Distinct());
            Assert.Single(result.Value.Value.WalletPasses.Select(x => x.AndroidBoardingPassWalletLink).Distinct());

            Assert.Null(result.Value.Value.PnrMultiBoardingPassUrl);
            Assert.Null(result.Value.Value.IosPnrMultiBoardingPassUrl);
            Assert.Null(result.Value.Value.AndroidPnrMultiBoardingPassUrl);
        }

        [Fact]
        public async Task Service_MultiPassengerSingleSegment_ReturnsPnrMultiBoardingPassUrls_AndKeepsAirshipPassLinks()
        {
            var request = CreateWalletPassRequest();
            request.Segments = request.Segments.Take(1).ToList();
            request.BoardingPasses = request.BoardingPasses.Where(x => x.OriginAirportCode.Is(request.Segments.First().Origin.Code)).ToList();

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            var result = await provider.GenerateWalletLinksAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);

            Assert.Equal(3, result.Value.Value.WalletPasses.Count());
            Assert.False(result.Value.Value.PnrMultiBoardingPassUrl.IsNullOrWhiteSpace());
            Assert.False(result.Value.Value.IosPnrMultiBoardingPassUrl.IsNullOrWhiteSpace());
            Assert.False(result.Value.Value.AndroidPnrMultiBoardingPassUrl.IsNullOrWhiteSpace());

            var pnrMultiUrl = result.Value.Value.PnrMultiBoardingPassUrl;
            var pnrMultiUri = new Uri(pnrMultiUrl);
            Assert.Equal("/v1/pass/adaptive", pnrMultiUri.AbsolutePath);
            Assert.Null(GetQueryValue(pnrMultiUri, "deviceType"));

            var pnrIds = GetQueryValue(pnrMultiUri, "ids");
            Assert.False(pnrIds.IsNullOrWhiteSpace());
            Assert.InRange(pnrIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Length, 1, 10);
            var pnrIdSet = pnrIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.Equal("ios", GetQueryValue(new Uri(result.Value.Value.IosPnrMultiBoardingPassUrl), "deviceType"));
            Assert.Equal("android", GetQueryValue(new Uri(result.Value.Value.AndroidPnrMultiBoardingPassUrl), "deviceType"));
            Assert.Equal(pnrIds, GetQueryValue(new Uri(result.Value.Value.IosPnrMultiBoardingPassUrl), "ids"));
            Assert.Equal(pnrIds, GetQueryValue(new Uri(result.Value.Value.AndroidPnrMultiBoardingPassUrl), "ids"));

            Assert.Equal(request.Passengers.Count(), result.Value.Value.WalletPasses.Select(x => x.BoardingPassAdaptiveLink).Distinct().Count());
            Assert.All(result.Value.Value.WalletPasses, x =>
            {
                Assert.NotNull(x.BoardingPassAdaptiveLink);
                Assert.NotNull(x.IosBoardingPassWalletLink);
                Assert.NotNull(x.AndroidBoardingPassWalletLink);

                // Single-segment flow should keep Airship-provided per-pass links (path-based /ios and /android),
                // while only the PNR multi-pass links are query-based with deviceType.
                Assert.EndsWith("/ios", x.IosBoardingPassWalletLink);
                Assert.EndsWith("/android", x.AndroidBoardingPassWalletLink);
                Assert.DoesNotContain("deviceType=", x.IosBoardingPassWalletLink, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("deviceType=", x.AndroidBoardingPassWalletLink, StringComparison.OrdinalIgnoreCase);

                var adaptiveId = ExtractAdaptiveIdFromAirshipUrl(x.BoardingPassAdaptiveLink);
                Assert.False(adaptiveId.IsNullOrWhiteSpace());
                Assert.True(pnrIdSet.Contains(adaptiveId));
            });
        }

        [Fact]
        public async Task Service_SinglePassengerSingleSegment_ReturnsNullPnrUrls_AndKeepsAirshipPassLinks()
        {
            var request = CreateWalletPassRequest();
            request.Segments = request.Segments.Take(1).ToList();
            request.Passengers = request.Passengers.Take(1).ToList();

            var firstPassenger = request.Passengers.First();
            var passengerNin = $"{firstPassenger.PassengerFirstNIN}.{firstPassenger.PassengerLastNIN}";

            request.BoardingPasses = request.BoardingPasses
                .Where(x =>
                    x.OriginAirportCode.Is(request.Segments.First().Origin.Code) &&
                    x.PassengerNIN.Is(passengerNin))
                .ToList();

            var provider = _serviceProvider.GetRequiredService<IBoardingPassWalletService>();

            var result = await provider.GenerateWalletLinksAsync(request);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);

            Assert.Single(result.Value.Value.WalletPasses);
            Assert.Null(result.Value.Value.PnrMultiBoardingPassUrl);
            Assert.Null(result.Value.Value.IosPnrMultiBoardingPassUrl);
            Assert.Null(result.Value.Value.AndroidPnrMultiBoardingPassUrl);

            var pass = result.Value.Value.WalletPasses.Single();
            Assert.NotNull(pass.BoardingPassAdaptiveLink);
            Assert.NotNull(pass.IosBoardingPassWalletLink);
            Assert.NotNull(pass.AndroidBoardingPassWalletLink);

            Assert.EndsWith("/ios", pass.IosBoardingPassWalletLink);
            Assert.EndsWith("/android", pass.AndroidBoardingPassWalletLink);
            Assert.DoesNotContain("deviceType=", pass.IosBoardingPassWalletLink, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("deviceType=", pass.AndroidBoardingPassWalletLink, StringComparison.OrdinalIgnoreCase);

            var adaptiveId = ExtractAdaptiveIdFromAirshipUrl(pass.BoardingPassAdaptiveLink);
            Assert.False(adaptiveId.IsNullOrWhiteSpace());
        }

        private static string GetQueryValue(Uri uri, string key)
        {
            if (uri == null || key.IsNullOrWhiteSpace())
                return null;

            var query = uri.Query;
            if (query.IsNullOrWhiteSpace())
                return null;

            foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length == 0)
                    continue;

                var pairKey = Uri.UnescapeDataString(pair[0]);
                if (!pairKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : string.Empty;
            }

            return null;
        }

        private static string ExtractAdaptiveIdFromAirshipUrl(string url)
        {
            if (url.IsNullOrWhiteSpace())
                return null;

            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 4)
                return null;

            // Expected: /v1/pass/adaptive/{id}
            if (!segments[0].Equals("v1", StringComparison.OrdinalIgnoreCase) ||
                !segments[1].Equals("pass", StringComparison.OrdinalIgnoreCase) ||
                !segments[2].Equals("adaptive", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return segments[3];
        }

        private static WalletPassRequest CreateWalletPassRequest()
        {
            var firstSegmentDepartureLocal = new LocalDateTime(2026, 7, 1, 18, 00, 0);
            var firstSegmentArrivalLocal = new LocalDateTime(2026, 7, 2, 6, 00, 0);

            var secondSegmentDepartureLocal = new LocalDateTime(2026, 7, 2, 10, 40, 0);
            var secondSegmentArrivalLocal = new LocalDateTime(2026, 7, 3, 0, 05, 0);

            return new WalletPassRequest
            {
                Airports =
                [
                    new()
                    {
                        AirportCode = "JFK",
                        AirportName = "John F Kennedy International",
                        CityCode = "NYC",
                        CityName = "New York",
                        CountryCode = "US",
                        CountryName = "NY",
                        TimeZone = "America/New_York"
                    },
                    new()
                    {
                        AirportCode = "LHR",
                        AirportName = "Heathrow Airport",
                        CityCode = "LON",
                        CityName = "London Heathrow",
                        CountryCode = "GB",
                        CountryName = "United Kingdom",
                        TimeZone = "Europe/London"
                    },
                    new()
                    {
                        AirportCode = "DEL",
                        AirportName = "Indira Gandhi International Airport",
                        CityCode = "DEL",
                        CityName = "Delhi",
                        CountryCode = "IN",
                        CountryName = "India",
                        TimeZone = "Asia/Calcutta"
                    }
                ],

                Segments =
                [
                    new()
                    {
                        Ordinal = 0,
                        OperatingCarrier = new FlightCarrier
                        {
                            CarrierCode = "VS",
                            CarrierName = "Virgin Atlantic",
                            FlightNum = "0004"
                        },
                        MarketingCarrier = new FlightCarrier
                        {
                            CarrierCode = "VS",
                            CarrierName = "Virgin Atlantic",
                            FlightNum = "0004"
                        },
                        ScheduledDepartureLocalDateTime = firstSegmentDepartureLocal,
                        ScheduledArrivalLocalDateTime = firstSegmentArrivalLocal,
                        Origin = new AirportAndTerminal
                        {
                            Code = "JFK",
                            AirportName = "John F Kennedy International",
                            CountryCode = "US"
                        },
                        Destination = new AirportAndTerminal
                        {
                            Code = "LHR",
                            AirportName = "Heathrow Airport",
                            CountryCode = "GB"
                        }
                    },
                    new()
                    {
                        Ordinal = 1,
                        OperatingCarrier = new FlightCarrier
                        {
                            CarrierCode = "VS",
                            CarrierName = "Virgin Atlantic",
                            FlightNum = "0302"
                        },
                        MarketingCarrier = new FlightCarrier
                        {
                            CarrierCode = "VS",
                            CarrierName = "Virgin Atlantic",
                            FlightNum = "0302"
                        },
                        ScheduledDepartureLocalDateTime = secondSegmentDepartureLocal,
                        ScheduledArrivalLocalDateTime = secondSegmentArrivalLocal,
                        Origin = new AirportAndTerminal
                        {
                            Code = "LHR",
                            AirportName = "Heathrow Airport",
                            CountryCode = "GB"
                        },
                        Destination = new AirportAndTerminal
                        {
                            Code = "DEL",
                            AirportName = "Indira Gandhi International Airport",
                            CountryCode = "IN"
                        }
                    }
                ],

                Passengers =
                [
                    new()
                    {
                        GivenNames = "PASSSIX",
                        Surname = "TEST",
                        PassengerFirstNIN = "01",
                        PassengerLastNIN = "01",
                        CheckedIn = true
                    },
                    new()
                    {
                        GivenNames = "PASSSEVEN",
                        Surname = "TEST",
                        PassengerFirstNIN = "01",
                        PassengerLastNIN = "02",
                        CheckedIn = true
                    },
                    new()
                    {
                        GivenNames = "PASSEIGHT",
                        Surname = "TEST",
                        PassengerFirstNIN = "01",
                        PassengerLastNIN = "03",
                        CheckedIn = true
                    }
                ],

                BoardingPasses = new List<BoardingPass>
                {
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSSIX",
                        Surname = "TEST",
                        PassengerNIN = "01.01",
                        SeatNumber = null,
                        Zone = null,
                        Terminal = "Terminal 4",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS4",
                        MarketingFlightNumber = "VS4",
                        DestinationAirportCode = "LHR",
                        OriginAirportCode = "JFK",
                        DepartureDateTime = DateTime.Parse("2026-07-01T18:00:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 1, 18, 0),
                        VerificationStatusMessage = "INTL - Verify Passport",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = null,
                        ShowSkyPriority = false,
                        ShowTsaPre = false,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSSIX        EDIQJAF JFKLHRVS 0004 182W    0003 756>518  W6182 VS              2A93221449342370                           NVS3JLJI53HYSBY",
                        BoardingTimeLocalTime = new LocalTime(17, 15),
                        TierCode = "VS_Gold"
                    },
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSSIX",
                        Surname = "TEST",
                        PassengerNIN = "01.01",
                        SeatNumber = "25H",
                        Zone = "GROUP2",
                        Terminal = "Terminal 3",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS302",
                        MarketingFlightNumber = "VS302",
                        DestinationAirportCode = "DEL",
                        OriginAirportCode = "LHR",
                        DepartureDateTime = DateTime.Parse("2026-07-02T10:40:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 2, 10, 40),
                        VerificationStatusMessage = "DOCS-OK",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = "TBA",
                        ShowSkyPriority = true,
                        ShowTsaPre = true,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSSIX        EDIQJAF LHRDELVS 0302 183W025H0001 156>518  W6182 VS              2A93221449342370                           NVS3JLJI53HY25H",
                        BoardingTimeLocalTime = new LocalTime(9, 45),
                        TierCode = "VS_Gold"
                    },
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSSEVEN",
                        Surname = "TEST",
                        PassengerNIN = "01.02",
                        SeatNumber = null,
                        Zone = null,
                        Terminal = "Terminal 4",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS4",
                        MarketingFlightNumber = "VS4",
                        DestinationAirportCode = "LHR",
                        OriginAirportCode = "JFK",
                        DepartureDateTime = DateTime.Parse("2026-07-01T18:00:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 1, 18, 0),
                        VerificationStatusMessage = "INTL - Verify Passport",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = null,
                        ShowSkyPriority = false,
                        ShowTsaPre = false,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSSEVEN      EDIQJAF JFKLHRVS 0004 182W    0002 756>518  W6182 VS              2A93221449342380                           NVS3JLJI53HQSBY",
                        BoardingTimeLocalTime = new LocalTime(17, 15),
                        TierCode = null
                    },
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSSEVEN",
                        Surname = "TEST",
                        PassengerNIN = "01.02",
                        SeatNumber = "25K",
                        Zone = "GROUP2",
                        Terminal = "Terminal 3",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS302",
                        MarketingFlightNumber = "VS302",
                        DestinationAirportCode = "DEL",
                        OriginAirportCode = "LHR",
                        DepartureDateTime = DateTime.Parse("2026-07-02T10:40:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 2, 10, 40),
                        VerificationStatusMessage = "INTL - Verify Passport",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = "TBA",
                        ShowSkyPriority = false,
                        ShowTsaPre = false,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSSEVEN      EDIQJAF LHRDELVS 0302 183W025K0002 156>518  W6182 VS              2A93221449342380                           NVS3JLJI53HQ25K",
                        BoardingTimeLocalTime = new LocalTime(9, 45),
                        TierCode = null
                    },
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSEIGHT",
                        Surname = "TEST",
                        PassengerNIN = "01.03",
                        SeatNumber = null,
                        Zone = null,
                        Terminal = "Terminal 4",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS4",
                        MarketingFlightNumber = "VS4",
                        DestinationAirportCode = "LHR",
                        OriginAirportCode = "JFK",
                        DepartureDateTime = DateTime.Parse("2026-07-01T18:00:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 1, 18, 0),
                        VerificationStatusMessage = "INTL - Verify Passport",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = null,
                        ShowSkyPriority = false,
                        ShowTsaPre = false,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSEIGHT      EDIQJAF JFKLHRVS 0004 182W    0001 756>518  W6182 VS              2A93221449342390                           NVS3JLJI53HISBY",
                        BoardingTimeLocalTime = new LocalTime(17, 15),
                        TierCode = "VS_Silver"
                    },
                    new()
                    {
                        PnrLocator = "DIQJAF",
                        GivenNames = "PASSEIGHT",
                        Surname = "TEST",
                        PassengerNIN = "01.03",
                        SeatNumber = "25F",
                        Zone = "GROUP2",
                        Terminal = "Terminal 3",
                        OperatingAirlineName = "Virgin Atlantic",
                        MarketingAirlineName = "Virgin Atlantic",
                        OperatingFlightNumber = "VS302",
                        MarketingFlightNumber = "VS302",
                        DestinationAirportCode = "DEL",
                        OriginAirportCode = "LHR",
                        DepartureDateTime = DateTime.Parse("2026-07-02T10:40:00Z"),
                        DepartureLocal = new LocalDateTime(2026, 7, 2, 10, 40),
                        VerificationStatusMessage = "INTL - Verify Passport",
                        TemplateCode = TemplateCode.VS_PREMIUM,
                        CabinName = "Premium",
                        Gate = "TBA",
                        ShowSkyPriority = false,
                        ShowTsaPre = false,
                        FlightStatusCode = FlightStatusCode.ON_TIME,
                        InfantName = " ",
                        CheckedIn = true,
                        QrCodeData = "M1TEST/PASSEIGHT      EDIQJAF LHRDELVS 0302 183W025F0003 156>518  W6182 VS              2A93221449342390                           NVS3JLJI53HI25F",
                        BoardingTimeLocalTime = new LocalTime(9, 45),
                        TierCode = "VS_Silver"
                    }
                }
            };
        }
    }
}
