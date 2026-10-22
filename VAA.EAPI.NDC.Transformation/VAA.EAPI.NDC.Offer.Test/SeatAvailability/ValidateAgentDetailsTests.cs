using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateAgentDetailsTests
    {
        [Fact]
        public void Should_throw_error_when_ParticipatingOrg_is_null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].ParticipatingOrg = null;

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_ParticipatingOrgId_is_empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].ParticipatingOrg.OrgID = string.Empty;

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain).ToList();

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_SalesAgent_is_empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].SalesAgent = new() { SalesAgentID = string.Empty };

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_SalesBranchId_is_empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].SalesBranch = new() { SalesBranchID = string.Empty };

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain).ToList();

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrdinalNo_is_empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = string.Empty;

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_7", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrdinalNo_is_zero_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "0";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrdinalNo_starts_with_letter_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "A";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OrdinalNo_not_Starts_with_one_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "2";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_not_throw_error_when_OrdinalNo_Starts_with_one_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            request.DistributionChain[0].Ordinal = "1";

            var validationErrors = ValidateSeatAvailability.ValidateAgentDetails(request.DistributionChain);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Should_throw_error_when_we_have_Invalid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_22", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Should_throw_error_code_G009_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "G009" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_22", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10146_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10146" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_3", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10140_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10140" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_6", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10144_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10144" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_2", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10147_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10147" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_4", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10152_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10152" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("368_8", validationErrors[0].ErrorCode);

        }

        [Fact]
        public void Should_throw_error_code_10153_in_Valid_response()
        {
            var validationRS = new ValidationRS() { Error = new ErrorDetails[] { new ErrorDetails() { Code = "10153" } } };
            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_44", validationErrors[0].ErrorCode);

        }
    }
}
