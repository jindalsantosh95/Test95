using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountDetailQueryValidatorTest
    {
        private readonly MemberAccountDetailQueryValidator _memberAccountDetailQueryValidator;
        public MemberAccountDetailQueryValidatorTest()
        {
            _memberAccountDetailQueryValidator = new MemberAccountDetailQueryValidator();
        }

        [Fact]
        public void ValidationSuccess()
        {
            var request = TestDataProvider.GetTestData<MemberAccountDetailsQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            Assert.True(_memberAccountDetailQueryValidator.Validate(request).IsValid);
        }
    }
}
