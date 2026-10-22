using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SupplementarySilverDelinkingCommandValidatorTest
    {
        private readonly SupplementarySilverDelinkingCommandValidator _supplementarySilverDelinkingCommandValidator;

        public SupplementarySilverDelinkingCommandValidatorTest()
        {
            _supplementarySilverDelinkingCommandValidator = new SupplementarySilverDelinkingCommandValidator();
        }

        [Fact]
        public void ValidationSuccess()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2"
            };
            var request = new SupplementarySilverDelinkingCommand()
            {
                InitiatedBy = "Test",
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            Assert.True(_supplementarySilverDelinkingCommandValidator.Validate(request).IsValid);
        }
    }
}
