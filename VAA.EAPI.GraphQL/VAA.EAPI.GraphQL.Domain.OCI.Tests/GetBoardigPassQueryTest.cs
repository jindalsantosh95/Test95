using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class GetBoardigPassQueryTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public GetBoardigPassQueryTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(["mobile;platform=ios;build-id=1.11.0(14)"]);

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                .AddSingleton<ITripsDetailsQueryProvider, TripsDetailsQueryProvider>()
                .AddSingleton<IWalletServiceProvider, WalletServiceProvider>()
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(sp => sp.GetRequiredService<TestTelemetryService>())
                .AddSingleton<GetBoardigPassQuery>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            var tripsDetailsQueryProvider = _serviceProvider.GetRequiredService<ITripsDetailsQueryProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();
            var walletServiceProvider = _serviceProvider.GetRequiredService<IWalletServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<GetBoardigPassQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(null, tripsDetailsQueryProvider, telemetryService, referenceServiceProvider, walletServiceProvider, logger));

            Assert.Equal("checkInQueryProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTripsDetailsQueryProviderIsNull()
        {
            var checkInQueryProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();
            var walletServiceProvider = _serviceProvider.GetRequiredService<IWalletServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<GetBoardigPassQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(checkInQueryProvider, null, telemetryService, referenceServiceProvider, walletServiceProvider, logger));

            Assert.Equal("tripsDetailsQueryProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryServiceProviderIsNull()
        {
            var checkInQueryProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var tripsDetailsQueryProvider = _serviceProvider.GetRequiredService<ITripsDetailsQueryProvider>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();
            var walletServiceProvider = _serviceProvider.GetRequiredService<IWalletServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<GetBoardigPassQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(checkInQueryProvider, tripsDetailsQueryProvider, null, referenceServiceProvider, walletServiceProvider, logger));

            Assert.Equal("telemetryService", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenReferenceServiceProviderIsNull()
        {
            var checkInQueryProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var tripsDetailsQueryProvider = _serviceProvider.GetRequiredService<ITripsDetailsQueryProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var walletServiceProvider = _serviceProvider.GetRequiredService<IWalletServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<GetBoardigPassQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(checkInQueryProvider, tripsDetailsQueryProvider, telemetryService, null, walletServiceProvider, logger));

            Assert.Equal("referenceServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenWalletServiceProviderIsNull()
        {
            var checkInQueryProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var tripsDetailsQueryProvider = _serviceProvider.GetRequiredService<ITripsDetailsQueryProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();
            var logger = _serviceProvider.GetRequiredService<ILogger<GetBoardigPassQuery>>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(checkInQueryProvider, tripsDetailsQueryProvider, telemetryService, referenceServiceProvider, null, logger));

            Assert.Equal("walletServiceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            var checkInQueryProvider = _serviceProvider.GetRequiredService<ICheckInServiceProvider>();
            var tripsDetailsQueryProvider = _serviceProvider.GetRequiredService<ITripsDetailsQueryProvider>();
            var telemetryService = _serviceProvider.GetRequiredService<ITelemetryService>();
            var referenceServiceProvider = _serviceProvider.GetRequiredService<IReferenceServiceProvider>();
            var walletServiceProvider = _serviceProvider.GetRequiredService<IWalletServiceProvider>();

            var exception = Assert.Throws<ArgumentNullException>(() => new GetBoardigPassQuery(checkInQueryProvider, tripsDetailsQueryProvider, telemetryService, referenceServiceProvider, walletServiceProvider, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task GetBoardingPass_WithoutNames_ShouldThrow()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>(); 
            var resolverContext = ResolverContextProxy.Create(out var context);

            await Assert.ThrowsAsync<ArgumentNullException>(() => { return _query.GetBoardingPass("DQL7ST", "LHR", null, null, context); });
        }

        [Fact]
        public async Task GetBoardingPass_WithNamesAndCheckedInPassenger_ShouldReturnResponse()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "DQL7ST", originCode: "LHR", givenNames: "VIRGIN", surname: "TESTONE", context);
            var boardingPass = Assert.Single(response.BoardingPasses, candidate =>
                candidate.PassengerCustomerId == "9999193289262"
                && candidate.OriginAirportCode == "LHR"
                && candidate.DestinationAirportCode == "JFK"
                && candidate.OperatingFlightNumber == "VS185");

            Assert.NotNull(response);
            Assert.True(boardingPass.CheckedIn);
            Assert.NotNull(boardingPass.Terminal);
            Assert.NotNull(boardingPass.SeatNumber);
            Assert.Equal("desktop-boarding-pass-lhr-jfk", boardingPass.DesktopBoardingPassBase64);

            var dayOfTravelEvent = Assert.Single(telemetryService.Events.Where(e => (LoggingCheckinEvent)e.Type == LoggingCheckinEvent.DAY_OF_TRAVEL));
            var telemetryBoardingPasses = JArray.Parse(Assert.IsType<JValue>(dayOfTravelEvent.Data["BoardingPasses"]).Value<string>()!);
            Assert.Contains(telemetryBoardingPasses, candidate =>
                candidate!["OriginCode"]?.Value<string>() == "LHR" &&
                candidate["DestinationCode"]?.Value<string>() == "JFK" &&
                candidate["HasQr"]?.Value<bool>() == true &&
                candidate["HasDesktopBoardingPass"]?.Value<bool>() == true);
        }

        [Fact]
        public async Task GetBoardingPass_MultiLegSegment_UsesLegMatchingSegmentOrigin()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "DQL7ST", originCode: "LHR", givenNames: "VIRGIN", surname: "TESTONE", context);
            var boardingPass = response.BoardingPasses.First();

            Assert.Equal("LHR", boardingPass.OriginAirportCode);
            Assert.NotNull(boardingPass.Terminal);
            Assert.NotNull(boardingPass.FlightStatusCode);
        }

        [Fact]
        public async Task GetBoardingPass_MultiPassenger_ReturnsLoyaltyDetails()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "DQL7ST", originCode: "LHR", givenNames: "VIRGIN", surname: "TESTONE", context);

            Assert.Null(response.TripsBookingId);
            Assert.DoesNotContain(response.BoardingPasses, x => x.OperationalStatus != null);

            //Non loyalty member
            var nonMemPsg = response.BoardingPasses.First();
            Assert.Null(nonMemPsg.TierCode);
            Assert.Null(nonMemPsg.TierName);

            //VS Gold Member
            var goldMemPsg = response.BoardingPasses.Skip(1).First();
            Assert.IsType<LoyaltyTierCode>(goldMemPsg.TierCode);
            Assert.Equal(LoyaltyTierCode.VS_Gold, goldMemPsg.TierCode);
            Assert.Contains("Gold", goldMemPsg.TierName);

            //VS Silver Member
            var silverMemPsg = response.BoardingPasses.Skip(2).First();
            Assert.IsType<LoyaltyTierCode>(silverMemPsg.TierCode);
            Assert.Equal(LoyaltyTierCode.VS_Silver, silverMemPsg.TierCode);
            Assert.Contains("Silver", silverMemPsg.TierName);

            //VS Red Member
            var redMemPsg = response.BoardingPasses.Skip(3).First();
            Assert.IsType<LoyaltyTierCode>(redMemPsg.TierCode);
            Assert.Equal(LoyaltyTierCode.VS_Red, redMemPsg.TierCode);
            Assert.Equal("Red", redMemPsg.TierName);

            //DL Gold Memeber
            var dlMemPsg = response.BoardingPasses.Skip(4).First();
            Assert.Null(dlMemPsg.TierCode);
            Assert.StartsWith("DL Gold", dlMemPsg.TierName);

            //KL Silver Member
            var klMemPsg = response.BoardingPasses.Skip(5).First();
            Assert.Null(klMemPsg.TierCode);
            Assert.Equal("KL Silver", klMemPsg.TierName);

            //AF club Member
            var afMemPsg = response.BoardingPasses.Skip(6).First();
            Assert.Null(afMemPsg.TierCode);
            Assert.Equal("AF Club 2000", afMemPsg.TierName);
        }


        [Fact]
        public async Task GetBoardingPass_MultiPassengersMultiSegment_ReturnsIropDetailsforCancelandDelayClean()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "EM6BVA", originCode: "JFK", givenNames: "IROP", surname: "TEST", context);

            Assert.NotNull(response.TripsBookingId);
            Assert.IsType<string>(response.TripsBookingId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(response.TripsBookingId));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload.ContainsKey("msBookingId"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload.ContainsKey("pnrLocator"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload.ContainsKey("givenName"));
            Assert.True(jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload.ContainsKey("surName"));
            Assert.Equal("EM6BVA", jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload["pnrLocator"]?.ToString());
            Assert.Contains(response.BoardingPasses, x => x.OperationalStatus != null);

            //cancelled clean IROP
            var cancelledCleanIropPass = response.BoardingPasses.FirstOrDefault(x => x.OperationalStatus != null && x.OperationalStatus.Type.Is("IROP") && x.OperationalStatus.SubType.Contains("Cancel"));

            Assert.NotNull(cancelledCleanIropPass);
            Assert.IsType<FlightSegmentStatus>(cancelledCleanIropPass.OperationalStatus);
            Assert.Equal("IROP Cancel Cleaned", cancelledCleanIropPass.OperationalStatus.SubType);
            Assert.True(cancelledCleanIropPass.OperationalStatus.RebookEligible);
            AssertIropContent(
                cancelledCleanIropPass.OperationalStatus,
                isCleaned: true,
                cardTitle: "Your flight has been rebooked",
                cardLines: ["We're sorry for the disruption.", "Tap to see your new flight."],
                pageTitle: "Your flight has been rebooked",
                firstSectionHeading: "We hope this new flight suits your travel plans.",
                nextActionText: ["View new flight", "See other options"]);

            //cdelayed clean IROP
            var delayedCleanIropPass = response.BoardingPasses.FirstOrDefault(x => x.OperationalStatus != null && x.OperationalStatus.Type.Is("IROP") && x.OperationalStatus.SubType.Contains("Delay"));

            Assert.NotNull(delayedCleanIropPass);
            Assert.IsType<FlightSegmentStatus>(delayedCleanIropPass.OperationalStatus);
            Assert.Equal("IROP Delay Cleaned", delayedCleanIropPass.OperationalStatus.SubType);
            Assert.True(delayedCleanIropPass.OperationalStatus.RebookEligible);
            AssertIropContent(
                delayedCleanIropPass.OperationalStatus,
                isCleaned: true,
                cardTitle: "Your flight has been rebooked",
                cardLines: ["We're sorry for the disruption.", "Tap to see your new flight."],
                pageTitle: "Your flight has been rebooked",
                firstSectionHeading: "We hope this new flight suits your travel plans.",
                nextActionText: ["View details", "See other options"]);
        }

        [Theory]
        [MemberData(nameof(IropContentScenarios))]
        public void FlightSegmentStatus_ConfiguredIropSubtype_ReturnsConfiguredInformation(
            string subType,
            string cardTitle,
            string[] cardLines,
            string pageTitle,
            string firstSectionHeading,
            string[] firstSectionParagraphs,
            string[] nextActionText,
            string finalParagraph,
            bool isCleaned)
        {
            var status = (FlightSegmentStatus)new Infrastructure.Models.Trips.OperationalStatus
            {
                Type = "IROP",
                SubType = subType
            };

            var information = Assert.IsType<IropInformation>(status.Information);
            var card = Assert.IsType<IropCard>(information.Card);
            var page = Assert.IsType<IropPage>(information.Page);

            Assert.Equal(cardTitle, card.Title);
            Assert.Equal(cardLines, card.Lines);
            Assert.Equal(pageTitle, page.Title);
            Assert.Equal(nextActionText, page.NextActionText);
            Assert.Equal(isCleaned, status.IsCleaned);

            var sections = page.Sections!.ToList();
            Assert.Equal(firstSectionHeading, sections[0].Heading);
            Assert.Equal(firstSectionParagraphs, sections[0].Paragraphs);
            Assert.Equal(finalParagraph, sections[1].Paragraphs!.Last());
        }

        [Fact]
        public void FlightSegmentStatus_OperationalStatusWithoutSubtype_ReturnsStatusWithoutInformation()
        {
            var status = (FlightSegmentStatus)new Infrastructure.Models.Trips.OperationalStatus
            {
                RebookEligible = true
            };

            Assert.Null(status.Type);
            Assert.Null(status.SubType);
            Assert.True(status.RebookEligible);
            Assert.False(status.IsCleaned);
            Assert.Null(status.Information);
        }

        [Fact]
        public void FlightSegmentStatus_IropStatusForOciWebDevChannel_ReturnsDefaultInformation()
        {
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(["oci-web-dev"]);

            var status = (FlightSegmentStatus)new Infrastructure.Models.Trips.OperationalStatus
            {
                Type = "IROP",
                SubType = "IROP Cancel Dirty Protect"
            };

            AssertIropContent(
                status,
                isCleaned: false,
                cardTitle: "Your flight has been cancelled",
                cardLines: ["We're sorry for the disruption.", "Click here to see your options."],
                pageTitle: "Your flight has been cancelled",
                firstSectionHeading: "We've found a new flight for you.",
                nextActionText: ["See your options"]);
        }

        [Fact]
        public void FlightSegmentStatus_IropStatusWithoutChannel_ReturnsDefaultInformation()
        {
            AsyncContext.Current = new AsyncContext();

            var status = (FlightSegmentStatus)new Infrastructure.Models.Trips.OperationalStatus
            {
                Type = "IROP",
                SubType = "IROP Cancel Dirty Protect"
            };

            AssertIropContent(
                status,
                isCleaned: false,
                cardTitle: "Your flight has been cancelled",
                cardLines: ["We're sorry for the disruption.", "Click here to see your options."],
                pageTitle: "Your flight has been cancelled",
                firstSectionHeading: "We've found a new flight for you.",
                nextActionText: ["See your options"]);
        }

        [Fact]
        public void FlightSegmentStatus_IropStatusForAndroidMobileChannel_ReturnsConfiguredInformation()
        {
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetChannel(["mobile;platform=android;build-id=1.0(684)"]);

            var status = (FlightSegmentStatus)new Infrastructure.Models.Trips.OperationalStatus
            {
                Type = "IROP",
                SubType = "IROP Cancel Dirty Protect"
            };

            AssertIropContent(
                status,
                isCleaned: false,
                cardTitle: "Your flight has been cancelled",
                cardLines: ["We're sorry for the disruption.", "Tap to see your options."],
                pageTitle: "Your flight has been cancelled",
                firstSectionHeading: "We've found a new flight for you.",
                nextActionText: ["See your options"]);
        }

        public static IEnumerable<object[]> IropContentScenarios =>
        [
            ["IROP Cancel Acknowledged", "Your flight has been rebooked", new[] { "We're sorry for the disruption.", "Tap to see your new flight." }, "Your flight has been rebooked", "We hope this new flight suits your travel plans.", new[] { "If this flight works for you, you're all set. You can check in on the app or at the airport.", "If this option doesn't suit your plans, other options may be available." }, new[] { "View new flight", "See other options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Cancel Cleaned", "Your flight has been rebooked", new[] { "We're sorry for the disruption.", "Tap to see your new flight." }, "Your flight has been rebooked", "We hope this new flight suits your travel plans.", new[] { "If this flight works for you, you're all set. You can check in on the app or at the airport.", "If this option doesn't suit your plans, other options may be available." }, new[] { "View new flight", "See other options" }, "We're sorry for any inconvenience caused to your travel plans.", true],
            ["IROP Delay Acknowledged", "Your flight has been rebooked", new[] { "We're sorry for the disruption.", "Tap to see your new flight." }, "Your flight has been rebooked", "We hope this new flight suits your travel plans.", new[] { "If this flight works for you, you’re all set.", "You can check in on the app or at the airport. If this option doesn't suit your plans, other options may be available." }, new[] { "View details", "See other options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Cleaned", "Your flight has been rebooked", new[] { "We're sorry for the disruption.", "Tap to see your new flight." }, "Your flight has been rebooked", "We hope this new flight suits your travel plans.", new[] { "If this flight works for you, you’re all set.", "You can check in on the app or at the airport. If this option doesn't suit your plans, other options may be available." }, new[] { "View details", "See other options" }, "We're sorry for any inconvenience caused to your travel plans.", true],
            ["IROP Cancel Dirty No Protect", "Your flight has been cancelled", new[] { "We're sorry for the disruption.", "Tap to see your options." }, "Your flight has been cancelled", "We're finding the next available flight.", new[] { "Once rebooked, your app will update automatically.", "If this option doesn't suit your plans, other options may be available." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Cancel Dirty Protect", "Your flight has been cancelled", new[] { "We're sorry for the disruption.", "Tap to see your options." }, "Your flight has been cancelled", "We've found a new flight for you.", new[] { "If this option works, you'll just need to confirm the change.", "If this option doesn't suit your plans, other options may be available." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Behind Schedule No Protect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We're working on the next steps and will keep your app updated.", new[] { "If this option doesn't suit your plans, other options may be available.", "If you're connecting to another flight in this booking, we'll offer options if the delay affects your connection." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Behind Schedule No Protect Misconnect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We're working on the next steps and will keep your app updated.", new[] { "If this option doesn't suit your plans, other options may be available.", "If you're connecting to another flight in this booking, we'll offer options if the delay affects your connection." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Dirty 15-30 No Protect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We're working on the next steps and will keep your app updated.", new[] { "If this option doesn't suit your plans, other options may be available.", "If you're connecting to another flight in this booking, we'll offer options if the delay affects your connection." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Dirty 15-30 No Protect Misconnect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We're working on the next steps and will keep your app updated.", new[] { "If this option doesn't suit your plans, other options may be available.", "If you're connecting to another flight in this booking, we'll offer options if the delay affects your connection." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Dirty 30 Up No Protect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We're working on the next steps and will keep your app updated.", new[] { "If this option doesn't suit your plans, other options may be available.", "If you're connecting to another flight in this booking, we'll offer options if the delay affects your connection." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false],
            ["IROP Delay Dirty Protect", "Your flight has been delayed", new[] { "We're sorry for the delay.", "Tap to see your updated flight details." }, "Your flight is delayed", "We've found an alternative flight option.", new[] { "If this works for you, you'll just need to confirm the change.", "If this option doesn't suit your plans, other options may be available." }, new[] { "See your options" }, "We're sorry for any inconvenience caused to your travel plans.", false]
        ];


        [Fact]
        public async Task GetBoardingPass_TwoPassengersSingleSegment_ReturnsIropDetailsforDelayDirtyProtect()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "EM6Y82", originCode: "LHR", givenNames: "IROP", surname: "TEST", context);

            Assert.NotNull(response.TripsBookingId);
            Assert.IsType<string>(response.TripsBookingId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(response.TripsBookingId));
            Assert.Equal("EM6Y82", jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload["pnrLocator"]?.ToString());
            Assert.Contains(response.BoardingPasses, x => x.OperationalStatus != null);
            Assert.Equal(2, response.BoardingPasses.Count());

            //delayed dirty IROP
            var delayedDirtyIropPass = response.BoardingPasses.FirstOrDefault(x => x.OperationalStatus != null && x.OperationalStatus.Type.Is("IROP") && x.OperationalStatus.SubType.Contains("Delay"));

            Assert.NotNull(delayedDirtyIropPass);
            Assert.IsType<FlightSegmentStatus>(delayedDirtyIropPass.OperationalStatus);
            Assert.Equal("IROP Delay Dirty Protect", delayedDirtyIropPass.OperationalStatus.SubType);
            Assert.Null(delayedDirtyIropPass.OperationalStatus.RebookEligible);
            AssertIropContent(
                delayedDirtyIropPass.OperationalStatus,
                isCleaned: false,
                cardTitle: "Your flight has been delayed",
                cardLines: ["We're sorry for the delay.", "Tap to see your updated flight details."],
                pageTitle: "Your flight is delayed",
                firstSectionHeading: "We've found an alternative flight option.",
                nextActionText: ["See your options"]);
        }

        [Fact]
        public async Task GetBoardingPass_SinglePassengerSingleSegment_ReturnsIropDetailsforCancelDirtyNoProtect()
        {
            var _query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var response = await _query.GetBoardingPass(pnrLocator: "EM56T2", originCode: "JFK", givenNames: "IROP", surname: "TEST", context);

            Assert.NotNull(response.TripsBookingId);
            Assert.IsType<string>(response.TripsBookingId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(response.TripsBookingId));
            Assert.Equal("EM56T2", jwtSecurityTokenHandler.ReadJwtToken(response.TripsBookingId).Payload["pnrLocator"]?.ToString());
            Assert.Contains(response.BoardingPasses, x => x.OperationalStatus != null);
            Assert.Single(response.BoardingPasses);

            //delayed dirty IROP
            var delayedDirtyIropPass = response.BoardingPasses.FirstOrDefault(x => x.OperationalStatus != null && x.OperationalStatus.Type.Is("IROP") && x.OperationalStatus.SubType.Contains("Cancel"));

            Assert.NotNull(delayedDirtyIropPass);
            Assert.IsType<FlightSegmentStatus>(delayedDirtyIropPass.OperationalStatus);
            Assert.Equal("IROP Cancel Dirty No Protect", delayedDirtyIropPass.OperationalStatus.SubType);
            Assert.Null(delayedDirtyIropPass.OperationalStatus.RebookEligible);
            AssertIropContent(
                delayedDirtyIropPass.OperationalStatus,
                isCleaned: false,
                cardTitle: "Your flight has been cancelled",
                cardLines: ["We're sorry for the disruption.", "Tap to see your options."],
                pageTitle: "Your flight has been cancelled",
                firstSectionHeading: "We're finding the next available flight.",
                nextActionText: ["See your options"]);
        }

        [Fact]
        public async Task GetBoardingPass_MultiPassengerMultiSegment_ReturnWalletLinksforCheckedPassengers()
        {
            var query = _serviceProvider.GetRequiredService<GetBoardigPassQuery>();
            _ = ResolverContextProxy.Create(out var context);

            var response = await query.GetBoardingPass(pnrLocator: "EJ2CXS", originCode: "LHR", givenNames: "O", surname: "TEST", context);

            Assert.NotNull(response);
            Assert.Equal(Constants.PassengersCheckInStatus.COMPLETED, response.CheckInStatus);
            Assert.Equal(6, response.BoardingPasses.Count());

            var expectedPnrMultiBoardingPassUrl = response.PnrMultiBoardingPassUrl;
            Assert.False(expectedPnrMultiBoardingPassUrl.IsNullOrWhiteSpace());
            Assert.NotNull(expectedPnrMultiBoardingPassUrl);

            var expectedPnrMultiUri = new Uri(expectedPnrMultiBoardingPassUrl);
            Assert.Equal("/v1/pass/adaptive", expectedPnrMultiUri.AbsolutePath);
            Assert.Null(GetQueryValue(expectedPnrMultiUri, "deviceType"));

            var expectedPnrIdsCsv = GetQueryValue(expectedPnrMultiUri, "ids");
            Assert.False(expectedPnrIdsCsv.IsNullOrWhiteSpace());

            var expectedPnrIds = expectedPnrIdsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.All(response.BoardingPasses, x =>
            {
                Assert.False(
                    x.IosBoardingPassWalletLink.IsNullOrWhiteSpace() &&
                    x.AndroidBoardingPassWalletLink.IsNullOrWhiteSpace() &&
                    x.BoardingPassAdaptiveLink.IsNullOrWhiteSpace());

                Assert.StartsWith("https://wallet-api.asnapieu.com", x.IosBoardingPassWalletLink ?? x.AndroidBoardingPassWalletLink ?? x.BoardingPassAdaptiveLink);

                if (!x.IosBoardingPassWalletLink.IsNullOrWhiteSpace())
                {
                    var iosUri = new Uri(x.IosBoardingPassWalletLink);
                    Assert.Equal("/v1/pass/adaptive", iosUri.AbsolutePath);
                    Assert.Equal("ios", GetQueryValue(iosUri, "deviceType"));
                    Assert.False(GetQueryValue(iosUri, "ids").IsNullOrWhiteSpace());
                }

                if (!x.AndroidBoardingPassWalletLink.IsNullOrWhiteSpace())
                {
                    var androidUri = new Uri(x.AndroidBoardingPassWalletLink);
                    Assert.Equal("/v1/pass/adaptive", androidUri.AbsolutePath);
                    Assert.Equal("android", GetQueryValue(androidUri, "deviceType"));
                    Assert.False(GetQueryValue(androidUri, "ids").IsNullOrWhiteSpace());
                }

                var adaptivePassIds = Array.Empty<string>();
                if (!x.BoardingPassAdaptiveLink.IsNullOrWhiteSpace())
                {
                    var adaptiveUri = new Uri(x.BoardingPassAdaptiveLink);
                    Assert.Equal("/v1/pass/adaptive", adaptiveUri.AbsolutePath);
                    Assert.Null(GetQueryValue(adaptiveUri, "deviceType"));

                    var adaptiveIdsCsv = GetQueryValue(adaptiveUri, "ids");
                    Assert.False(adaptiveIdsCsv.IsNullOrWhiteSpace());

                    adaptivePassIds = adaptiveIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                }

                foreach (var adaptivePassId in adaptivePassIds)
                {
                    Assert.Contains(adaptivePassId.Trim(), expectedPnrIds);
                }
            });
        }

        private static void AssertIropContent(
            FlightSegmentStatus status,
            bool isCleaned,
            string cardTitle,
            string[] cardLines,
            string pageTitle,
            string firstSectionHeading,
            string[] nextActionText)
        {
            var information = Assert.IsType<IropInformation>(status.Information);
            var card = Assert.IsType<IropCard>(information.Card);
            var page = Assert.IsType<IropPage>(information.Page);

            Assert.Equal(isCleaned, status.IsCleaned);
            Assert.Equal(cardTitle, card.Title);
            Assert.Equal(cardLines, card.Lines);
            Assert.Equal(pageTitle, page.Title);
            Assert.Equal(firstSectionHeading, page.Sections!.First().Heading);
            Assert.Equal(nextActionText, page.NextActionText);
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
    }
}
