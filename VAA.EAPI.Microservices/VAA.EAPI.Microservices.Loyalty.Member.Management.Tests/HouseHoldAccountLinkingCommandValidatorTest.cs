using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountLinkingCommandValidatorTest
    {
        private readonly HouseHoldAccountLinkingCommandValidator _houseHoldAccountLinkingCommandValidator;
        public HouseHoldAccountLinkingCommandValidatorTest()
        {
            _houseHoldAccountLinkingCommandValidator = new HouseHoldAccountLinkingCommandValidator();
        }

        [Fact]
        public void ValidationSuccess()
        {
            var request = TestDataProvider.GetTestData<HouseHoldAccountLinkingCommand>("MemberManagement\\HouseHoldAccountLinkingRequest");
            Assert.True(_houseHoldAccountLinkingCommandValidator.Validate(request).IsValid);
        }
    }
}
