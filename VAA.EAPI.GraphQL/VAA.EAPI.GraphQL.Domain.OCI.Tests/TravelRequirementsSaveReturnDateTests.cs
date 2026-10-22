using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class TravelRequirementsSaveReturnDateTests
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly TravelRequirementsSaveReturnDateMutation _mutation;
        private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

        public TravelRequirementsSaveReturnDateTests()
        {
            Setup.CreateTestEnvironmentSettings();
            _httpClientFactoryFixture = new HttpClientFactoryFixture();

            IHttpConnector httpConnector = new HttpConnector(NullLogger<HttpConnector>.Instance, _httpClientFactoryFixture.Current);
            ITravelRequirementsServiceProvider travelRequirementsServiceProvider = new TravelRequirementsServiceProvider(httpConnector);
            ITelemetryService telemetryService = new TelemetryService(new TelemetryClient(new TelemetryConfiguration()), NullLogger<TelemetryService>.Instance);

            _mutation = new TravelRequirementsSaveReturnDateMutation(
                travelRequirementsServiceProvider,
                telemetryService,
                NullLogger<TravelRequirementsSaveReturnDateMutation>.Instance);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForNullRequest()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveReturnDate(null, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Mandatory field missing.", error.Message);
            Assert.Empty(GetSaveReturnDateRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingReturnDate()
        {
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", ["passenger-1"]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveReturnDate(new TravelRequirementsSaveReturnDateRequest
                {
                    TravelRequirementsId = travelRequirementsId,
                    PassengerId = "passenger-1",
                    NoReturnDate = false
                }, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): ReturnOn", error.Message);
            Assert.Empty(GetSaveReturnDateRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnRefreshedTravelRequirementsResponse_ForProvidedDate()
        {
            var passengerId = "01.01|resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);

            var result = await _mutation.SaveReturnDate(new TravelRequirementsSaveReturnDateRequest
                {
                    TravelRequirementsId = travelRequirementsId,
                    PassengerId = passengerId,
                    ReturnOn = new LocalDate(2025, 8, 29),
                    NoReturnDate = false
                }, null);

            Assert.True(_jwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));
            Assert.Equal(2, result.Passengers.Count());
            Assert.Equal(2, result.Slices.Count());

            var affectedRequirement = GetAffectedRequirement(result, passengerId);
            Assert.Equal(ReturnDateStatus.PROVIDED, affectedRequirement.ReturnDateStatus);
            Assert.Equal(new LocalDate(2025, 8, 29), affectedRequirement.ReturnDate);

            var payload = await GetSaveReturnDateRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains("\"returnOn\":\"2025-08-29\"", payload);
            Assert.DoesNotContain("\"noReturnDate\":true", payload);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorAfterSendingDependencyRequest_ForProviderBusinessError()
        {
            var passengerId = "01.01|business-error-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveReturnDate(new TravelRequirementsSaveReturnDateRequest
                {
                    TravelRequirementsId = travelRequirementsId,
                    PassengerId = passengerId,
                    ReturnOn = new LocalDate(2025, 8, 29),
                    NoReturnDate = false
                }, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(TravelRequirementsErrorMessages.DefaultFailureMessage, error.Message);

            var payload = await GetSaveReturnDateRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains("\"returnOn\":\"2025-08-29\"", payload);
            Assert.DoesNotContain("\"noReturnDate\":true", payload);
        }

        [Fact]
        public async Task Mutation_ShouldAllowNoReturnDateAndReturnDeclinedRequirementState()
        {
            var passengerId = "01.01|declined-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);

            var result = await _mutation.SaveReturnDate(new TravelRequirementsSaveReturnDateRequest
                {
                    TravelRequirementsId = travelRequirementsId,
                    PassengerId = passengerId,
                    NoReturnDate = true
                }, null);

            var affectedRequirement = GetAffectedRequirement(result, passengerId);
            Assert.Equal(ReturnDateStatus.REQUIRED, affectedRequirement.ReturnDateStatus);
            Assert.True(affectedRequirement.IsReturnDateDeclined);
            Assert.Null(affectedRequirement.ReturnDate);

            var payload = await GetSaveReturnDateRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.DoesNotContain("\"returnOn\":", payload);
            Assert.Contains("\"noReturnDate\":true", payload);
        }

        private async Task<string> GetSaveReturnDateRequestPayload(string passengerId)
        {
            var payloads = new List<string>();

            foreach (var request in GetSaveReturnDateRequestPayloads())
            {
                payloads.Add(await request.Content!.ReadAsStringAsync(CancellationToken.None));
            }

            return payloads
                .Single(payload => payload.Contains($"\"passengerId\":\"{passengerId}\"", StringComparison.Ordinal));
        }

        private static CheckInRequirements GetAffectedRequirement(TravelRequirementsResponse result, string passengerId)
        {
            var passenger = result.Passengers.Single(x => x.PassengerId == passengerId);

            return passenger.SliceRequirements!.Single();
        }

        private IEnumerable<HttpRequestMessage> GetSaveReturnDateRequestPayloads()
        {
            return _httpClientFactoryFixture.Current
                .GetResponses($"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveReturnDate")
                .Select(response => response.RequestMessage)
                .Where(request => request?.RequestUri?.AbsolutePath.EndsWith("/TravelRequirements/SaveReturnDate", StringComparison.OrdinalIgnoreCase) == true)
                .Select(request => request!);
        }

    }
}
