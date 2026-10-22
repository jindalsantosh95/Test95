using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnerDataManagementMutationTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnerDataManagementMutation(null));
        }

        [Fact]
        public async Task EditVSRetroFlownData_ShouldForwardRequestAndReturnProviderResponse()
        {
            var provider = new CapturingPartnerDataManagementProvider
            {
                UpdateResponse = new EditVSRetroFlownDataResponse
                {
                    SuccessMessage = "VS retro flown details updated successfully."
                }
            };
            var mutation = new PartnerDataManagementMutation(provider);
            var request = new EditVSRetroFlownDataRequest
            {
                OriginAirport = "LHR",
                DestinationAirport = "JFK",
                TicketNumber = "9321234567890",
                Status = "Allocated",
                VALoyaltyProgramCode = "VS",
                MemberNumber = "1234567890"
            };

            var result = await mutation.EditVSRetroFlownData(request);

            Assert.Same(request, provider.LastUpdateRequest);
            Assert.Equal("VS retro flown details updated successfully.", result.SuccessMessage);
        }

        [Fact]
        public async Task EditVSRetroFlownData_ShouldThrow_WhenRequestIsNull()
        {
            var mutation = new PartnerDataManagementMutation(new CapturingPartnerDataManagementProvider());

            await Assert.ThrowsAsync<ArgumentNullException>(() => mutation.EditVSRetroFlownData(null));
        }

        [Theory]
        [InlineData(null, "JFK", "Allocated", "9321234567890")]
        [InlineData("LHR", " ", "Allocated", "9321234567890")]
        [InlineData("LHR", "JFK", "", "9321234567890")]
        [InlineData("LHR", "JFK", "Allocated", "  ")]
        public async Task EditVSRetroFlownData_ShouldThrow_WhenMandatoryInputIsMissing(
            string? originAirport,
            string? destinationAirport,
            string? status,
            string? ticketNumber)
        {
            var provider = new CapturingPartnerDataManagementProvider();
            var mutation = new PartnerDataManagementMutation(provider);
            var request = new EditVSRetroFlownDataRequest
            {
                OriginAirport = originAirport,
                DestinationAirport = destinationAirport,
                TicketNumber = ticketNumber,
                Status = status,
                VALoyaltyProgramCode = "VS",
                MemberNumber = "1234567890"
            };

            await Assert.ThrowsAsync<RequestValidationException>(() => mutation.EditVSRetroFlownData(request));
            Assert.Null(provider.LastUpdateRequest);
        }

        private class CapturingPartnerDataManagementProvider : IPartnerDataManagementQueryProvider
        {
            public EditVSRetroFlownDataRequest? LastUpdateRequest { get; private set; }

            public EditVSRetroFlownDataResponse UpdateResponse { get; init; } = new();

            public Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request) => throw new NotSupportedException();

            public Task<EditVSRetroFlownDataResponse> UpdateVSRetroFlownDetails(EditVSRetroFlownDataRequest request)
            {
                LastUpdateRequest = request;
                return Task.FromResult(UpdateResponse);
            }

            public Task<ValidateVSClaimInputsResponse> ValidateVSClaimInputs(ValidateVSClaimInputsRequest request) => throw new NotSupportedException();

            public Task<string> UpdateVSRetroFlownStatus(TicketStatusRequest request) => throw new NotSupportedException();
        }
    }
}
