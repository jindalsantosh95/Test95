using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class PromotionEnrolledMemberDetailsQueryValidatorTest
    {
        private readonly PromotionEnrolledMemberDetailsQueryValidator _promotionEnrolledMemberDetailsQueryValidator;
        public PromotionEnrolledMemberDetailsQueryValidatorTest()
        {
            _promotionEnrolledMemberDetailsQueryValidator = new PromotionEnrolledMemberDetailsQueryValidator();
        }

        [Fact]
        public void ValidationSuccess()
        {
            var request = TestDataProvider.GetTestData<PromotionEnrolledMemberDetailsQuery>("MemberManagement\\EnrolledMemberForPromotionRequest");
            Assert.True(_promotionEnrolledMemberDetailsQueryValidator.Validate(request).IsValid);
        }
    }
}
