using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class ValidatePasswordCommandHandlerTests
    {
        [Fact]
        public async Task Handle_GoodPassword_ShouldSucceed()
        {
            // Arrange
            ValidatePasswordCommandHandler _classUnderTest = new();
            ValidatePasswordCommand command = new()
            {
                Password = new()
                {
                    Password = "A Good one 23!"
                }
            };

            // Act
            var testOutput = await _classUnderTest.Handle(command, System.Threading.CancellationToken.None);

            // Assert
            Assert.NotNull(testOutput);
            foreach (var pp in testOutput.Value)
                Assert.True(pp.IsValid, $"Failed {pp.PolicyDescription}");
        }

        [Theory]
        [InlineData("Shrt1!£", "PP01")]
        [InlineData("ThisPasswordIsWayTooLong1!£", "PP01")]
        [InlineData("MissingDigits!£", "PP02")]
        [InlineData("missingupper1$", "PP03")]
        [InlineData("MISSINGLOWER1£", "PP04")]
        [InlineData("ManySymbols123!£$%", "PP05")]
        [InlineData("Has@Symbol123", "PP06")]
        public async Task Handle_EachPolicy_ShouldSucceed(string password, string policyId)
        {
            // Arrange
            var classUnderTest = new ValidatePasswordCommandHandler();
            ValidatePasswordCommand command = new()
            {
                Password = new()
                {
                    Password = password
                }
            };

            // Act
            var testOutput = await classUnderTest.Handle(command, System.Threading.CancellationToken.None);

            // Assert
            Assert.NotNull(testOutput);


            var ppTest = testOutput.Value.FirstOrDefault(p => p.PolicyId.Equals(policyId));
            Assert.True(ppTest != null, $"{policyId} not returned in response");

            foreach (var pp in testOutput.Value)
            {
                bool expected = !(pp.PolicyId == policyId);
                Assert.True(pp.IsValid == expected, $"{pp.PolicyId} | {pp.PolicyDescription} should have returned {expected}");

            }
        }
    }
}
