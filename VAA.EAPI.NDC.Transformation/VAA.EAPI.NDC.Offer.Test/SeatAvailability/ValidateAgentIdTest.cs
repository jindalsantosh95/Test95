using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateAgentIdTest
    {
        [Fact]
        public void Should_throw_Error_when_AgentId_is_null_in_request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\SeatAvailability\\ValidationResponse");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");
            validationRS.ValidateResponse.AgencyDetails.RequestAgencyId = null;

            var validationErrors = RequestValidations.ValidateAgentID(validationRS.ValidateResponse, orderDoc);
            
            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("368_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Should_not_throw_Error_when_AgentId_is_valid_in_request()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\SeatAvailability\\ValidationResponse");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");            

            var validationErrors = RequestValidations.ValidateAgentID(validationRS.ValidateResponse, orderDoc);

            Assert.False(validationErrors.Any());
        }
      
        [Fact]
        public void Should_not_throw_Error_when_OrderDoc_is_null()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\SeatAvailability\\ValidationResponse");

            var validationErrors = RequestValidations.ValidateAgentID(validationRS.ValidateResponse, null);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Should_not_throw_Error_when_OrderDoc_is_found()
        {
            var validationRS = TestDataProvider.GetTestData<ValidationRS>("NDCOffer\\SeatAvailability\\ValidationResponse");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\SeatAvailability\\OrderDoc");

            var validationErrors = RequestValidations.ValidateAgentID(validationRS.ValidateResponse, orderDoc);

            Assert.False(validationErrors.Any()); 
        }

       
    }
}
