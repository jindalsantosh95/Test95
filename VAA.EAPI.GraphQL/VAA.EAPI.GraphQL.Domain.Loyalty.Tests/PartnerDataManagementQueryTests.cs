using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnerDataManagementQueryTests
    {
        [Fact]
        public void VSFlownDetailsResponse_ShouldInitializeEmptyDetails()
        {
            var response = new VSFlownDetailsResponse();

            Assert.NotNull(response.VSRetroFlownDetails);
            Assert.Empty(response.VSRetroFlownDetails);
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerDataManagementQuery(null));
        }

        [Fact]
        public async Task GetVSRetroFlownData_ShouldThrow_WhenRequestIsNull()
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IPartnerDataManagementQueryProvider>(new CapturingPartnerDataManagementQueryProvider())
                .AddSingleton<PartnerDataManagementQuery>()
                .BuildServiceProvider();

            var query = serviceProvider.GetRequiredService<PartnerDataManagementQuery>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => query.GetVSRetroFlownData(null));
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        [InlineData(null, "ABC123", null)] // pnr only
        [InlineData(null, null, "1234")] // flight only
        [InlineData(null, "ABC123", " ")] // flight missing
        [InlineData(null, " ", "1234")] // pnr missing
        public async Task GetVSRetroFlownData_ShouldThrow_WhenRequestDoesNotMeetInputRules(string? ticketNumber, string? pnr, string? flightNumber)
        {
            var request = new GetVSRetroFlownDataRequest
            {
                TicketNumber = ticketNumber,
                Pnr = pnr,
                FlightNumber = flightNumber
            };

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IPartnerDataManagementQueryProvider>(new CapturingPartnerDataManagementQueryProvider())
                .AddSingleton<PartnerDataManagementQuery>()
                .BuildServiceProvider();

            var query = serviceProvider.GetRequiredService<PartnerDataManagementQuery>();
            await Assert.ThrowsAsync<ArgumentException>(() => query.GetVSRetroFlownData(request));
        }

        [Fact]
        public async Task GetVSRetroFlownData_ShouldSucceed_WhenTicketNumberProvided()
        {
            var request = new GetVSRetroFlownDataRequest
            {
                TicketNumber = "1234567890",
                Pnr = null,
                FlightNumber = null
            };

            var expected = new VSFlownDetailsResponse { VSRetroFlownDetails = new List<VSRetroFlownDetail> { new() { Id = 1, TicketNumber = "1234567890" } } };

            var provider = new CapturingPartnerDataManagementQueryProvider { Response = expected };

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IPartnerDataManagementQueryProvider>(provider)
                .AddSingleton<PartnerDataManagementQuery>()
                .BuildServiceProvider();

            var query = serviceProvider.GetRequiredService<PartnerDataManagementQuery>();
            var result = await query.GetVSRetroFlownData(request);

            Assert.NotNull(result);
            Assert.IsType<VSFlownDetailsResponse>(result);
            Assert.NotNull(result.VSRetroFlownDetails);
            Assert.Single(result.VSRetroFlownDetails);

            Assert.Equal("1234567890", provider.LastRequest?.TicketNumber);
        }

        [Fact]
        public async Task GetVSRetroFlownData_ShouldSucceed_WhenPnrAndFlightNumberProvided()
        {
            var request = new GetVSRetroFlownDataRequest
            {
                TicketNumber = null,
                Pnr = "ABC123",
                FlightNumber = "0012"
            };

            var expected = new VSFlownDetailsResponse { VSRetroFlownDetails = new List<VSRetroFlownDetail> { new() { Id = 1, RecordLocatorOperatingCarrier = "ABC123" } } };

            var provider = new CapturingPartnerDataManagementQueryProvider { Response = expected };

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IPartnerDataManagementQueryProvider>(provider)
                .AddSingleton<PartnerDataManagementQuery>()
                .BuildServiceProvider();

            var query = serviceProvider.GetRequiredService<PartnerDataManagementQuery>();
            var result = await query.GetVSRetroFlownData(request);

            Assert.NotNull(result);
            Assert.IsType<VSFlownDetailsResponse>(result);
            Assert.NotNull(result.VSRetroFlownDetails);
            Assert.Single(result.VSRetroFlownDetails);

            Assert.Equal("ABC123", provider.LastRequest?.Pnr);
            Assert.Equal("0012", provider.LastRequest?.FlightNumber);
        }

        private sealed class CapturingPartnerDataManagementQueryProvider : IPartnerDataManagementQueryProvider
        {
            public VSFlownDetailsResponse Response { get; init; } = new() { VSRetroFlownDetails = new List<VSRetroFlownDetail>() };

            public GetVSRetroFlownDataRequest? LastRequest { get; private set; }

            public Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request)
            {
                LastRequest = request;
                return Task.FromResult(Response);
            }

            public Task<ValidateVSClaimInputsResponse> ValidateVSClaimInputs(ValidateVSClaimInputsRequest request)
            {
                throw new NotSupportedException("The PDM validation endpoint is not used by flown-data queries.");
            }

            public Task<EditVSRetroFlownDataResponse> UpdateVSRetroFlownDetails(EditVSRetroFlownDataRequest request)
            {
                throw new NotSupportedException("The PDM update endpoint is not used by flown-data queries.");
            }

            public Task<string> UpdateVSRetroFlownStatus(TicketStatusRequest request)
            {
                return Task.FromResult(string.Empty);
            }
        }
    }
}
