using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SaveContactTracingTravelRequirementsTests
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly SaveContactTracingMutation _mutation;
        private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

        public SaveContactTracingTravelRequirementsTests()
        {
            Setup.CreateTestEnvironmentSettings();
            _httpClientFactoryFixture = new HttpClientFactoryFixture();

            IHttpConnector httpConnector = new HttpConnector(NullLogger<HttpConnector>.Instance, _httpClientFactoryFixture.Current);
            ITravelRequirementsServiceProvider travelRequirementsServiceProvider = new TravelRequirementsServiceProvider(httpConnector);
            ITelemetryService telemetryService = new TelemetryService(new TelemetryClient(new TelemetryConfiguration()), NullLogger<TelemetryService>.Instance);

            _mutation = new SaveContactTracingMutation(
                travelRequirementsServiceProvider,
                telemetryService,
                NullLogger<SaveContactTracingMutation>.Instance);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingRequiredArguments()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveContactTracingTravelRequirements(null, null, null, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): travelRequirementsId, sliceId, contactTracing", error.Message);
            Assert.Empty(GetSaveContactTracingRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingRequiredAddressFields()
        {
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", ["01.01|resource-id"]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveContactTracingTravelRequirements(
                travelRequirementsId,
                "trip-id-1",
                new[]
                {
                    new ContactTracing
                    {
                        PassengerId = "01.01|resource-id",
                        StreetAddress = "123 Test Street",
                        City = "New York",
                        PrimaryPhoneNumber = "5550100",
                        PrimaryPhoneNumberCountryCode = "1",
                        NoEmail = true,
                        NoSecondaryPhoneNumber = true
                    }
                },
                context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): contactTracing.State, contactTracing.Zip", error.Message);
            Assert.Empty(GetSaveContactTracingRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnRefreshedTravelRequirementsResponse_ForContactTracing()
        {
            var passengerId = "01.01|resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);

            var result = await _mutation.SaveContactTracingTravelRequirements(travelRequirementsId, "trip-id-1", [CreateValidContactTracing(passengerId)], null);

            Assert.True(_jwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));
            Assert.Equal(2, result.Passengers.Count());
            Assert.Equal(2, result.Slices.Count());

            var passenger = result.Passengers.Single(x => x.PassengerId == passengerId);
            Assert.Equal(ContactTracingStatus.COMPLETE, passenger.SliceRequirements!.Single().ContactTracing);
            Assert.Null(passenger.SliceRequirements.Single().ContactTracingAvailableFromUtc);
            Assert.Null(passenger.SliceRequirements.Single().ContactTracingAvailableFromDescription);
            Assert.Equal("123 Test Street", passenger.ContactTracing!.StreetAddress);
            Assert.Equal("10001", passenger.ContactTracing.Zip);

            var unchangedPassenger = result.Passengers.Single(x => x.PassengerId == "02.01|other-resource-id");
            Assert.Null(unchangedPassenger.ContactTracing);
            Assert.Equal("slice-2", unchangedPassenger.SliceRequirements!.Single().SliceId);
            Assert.Equal(ContactTracingStatus.NOT_REQUIRED, unchangedPassenger.SliceRequirements.Single().ContactTracing);
            Assert.Null(unchangedPassenger.SliceRequirements.Single().ContactTracingAvailableFromUtc);
            Assert.Null(unchangedPassenger.SliceRequirements.Single().ContactTracingAvailableFromDescription);

            var payload = await GetSaveContactTracingRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains("\"sliceId\":\"trip-id-1\"", payload);
            Assert.Contains("\"contactTracing\":[", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains("\"streetAddress\":\"123 Test Street\"", payload);
            Assert.Contains("\"primaryPhoneNumber\":\"5550100\"", payload);
            Assert.DoesNotContain("\"applyToAll", payload, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorAfterSendingDependencyRequest_ForProviderBusinessError()
        {
            var passengerId = "01.01|business-error-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveContactTracingTravelRequirements(travelRequirementsId, "trip-id-1", [CreateValidContactTracing(passengerId)], context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("We can’t load the travel documents step at the moment. Please try again shortly.", error.Message);

            var payload = await GetSaveContactTracingRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains("\"sliceId\":\"trip-id-1\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
        }

        private static ContactTracing CreateValidContactTracing(string passengerId)
        {
            return new ContactTracing
            {
                PassengerId = passengerId,
                StreetAddress = "123 Test Street",
                City = "New York",
                State = "NY",
                Zip = "10001",
                Email = "traveller@example.test",
                PrimaryPhoneNumber = "5550100",
                PrimaryPhoneNumberCountryCode = "1",
                SecondaryPhoneNumber = "5550101",
                SecondaryPhoneNumberCountryCode = "1",
                NoEmail = false,
                NoSecondaryPhoneNumber = false,
                InTransit = false
            };
        }

        private async Task<string> GetSaveContactTracingRequestPayload(string passengerId)
        {
            var payloads = new List<string>();

            foreach (var request in GetSaveContactTracingRequestPayloads())
            {
                payloads.Add(await request.Content!.ReadAsStringAsync(CancellationToken.None));
            }

            return payloads
                .Single(payload => payload.Contains($"\"passengerId\":\"{passengerId}\"", StringComparison.Ordinal));
        }

        private IEnumerable<HttpRequestMessage> GetSaveContactTracingRequestPayloads()
        {
            return _httpClientFactoryFixture.Current
                .GetResponses($"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveContactTracing")
                .Select(response => response.RequestMessage)
                .Where(request => request?.RequestUri?.AbsolutePath.EndsWith("/TravelRequirements/SaveContactTracing", StringComparison.OrdinalIgnoreCase) == true)
                .Select(request => request!);
        }

    }
}
