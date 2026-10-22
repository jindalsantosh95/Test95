using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Mutations
{
    public class SelectSpecialAssistanceMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ISSRDetailsProvider _mockSSRDetailsQueryProvider;
        private readonly SelectSpecialAssistanceMutation _selectSpecialAssistanceMutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly ILogger<SelectSpecialAssistanceMutation> _mutationLogger;

        public SelectSpecialAssistanceMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _mutationLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<SelectSpecialAssistanceMutation>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockSSRDetailsQueryProvider = new SSRDetailsProvider(_httpConnector);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _selectSpecialAssistanceMutation = new SelectSpecialAssistanceMutation(_mockSSRDetailsQueryProvider, _mockReferenceServiceProvider, _mutationLogger);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task EditOneEligibleSegmentSpecialAssistanceForTrip_ValidateSelection()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";


            // Act
            var result = await _selectSpecialAssistanceMutation.SelectSpecialAssistance(tripReservationId, segmentIds: ["1"], passengerId: passengerId, ssrCode: "WCHC", ssrCategoryCode: "WCH");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").ServiceRequests.ToList().First(sr => sr.CategoryCode == "WCH").CategoryDetails.First(cd => cd.ServiceCode == "WCHC").IsSelected == true);
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").ServiceRequests.ToList().First(sr => sr.CategoryCode == "WCH").CategoryDetails.First(cd => cd.ServiceCode == "NASS").IsSelected == false);
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").ServiceRequests.ToList().First(sr => sr.CategoryCode == "WCH").CategoryDetails.First(cd => cd.ServiceCode == "WCHC").Features.Any() == true);
        }

        [Fact]
        public async Task DeleteOneEligibleSegmentSpecialAssistanceForTrip_ValidateDeletion()
        {
            // Arrange
            var tripReservationId = BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac5", "givenName", "surName", "ABCDEF", null, Guid.NewGuid().ToString()).GetAwaiter().GetResult();
            var passengerId = "0f02a6d8-d45a-49fb-a664-a36c8e01e905";


            // Act
            var result = await _selectSpecialAssistanceMutation.SelectSpecialAssistance(tripReservationId, segmentIds: ["1"], passengerId: passengerId, ssrCode: "NASS", ssrCategoryCode: "WCH");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").ServiceRequests.ToList().First(sr => sr.CategoryCode == "WCH").CategoryDetails.First(cd => cd.ServiceCode == "NASS").IsSelected == true);
            Assert.True(result.Passengers.First(p => p.PassengerResourceId == passengerId).PassengerSegments.First(ps => ps.FlightSegment.SegmentId == "1").ServiceRequests.ToList().First(sr => sr.CategoryCode == "WCH").CategoryDetails.First(cd => cd.ServiceCode == "WCHC").IsSelected == false);
        }
    }
}
