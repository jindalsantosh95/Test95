using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountDelinkingCommandValidatorTest
    {
        private readonly HouseHoldAccountDelinkingCommandValidator _houseHoldAccountDelinkingCommandValidator;
        public HouseHoldAccountDelinkingCommandValidatorTest()
        {
            _houseHoldAccountDelinkingCommandValidator = new HouseHoldAccountDelinkingCommandValidator();
        }

        [Fact]
        public void ValidationSuccess()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            var request = new HouseHoldAccountDelinkingCommand()
            {
                InitiatedBy = "Test",
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            Assert.True(_houseHoldAccountDelinkingCommandValidator.Validate(request).IsValid);
        }
    }
}
