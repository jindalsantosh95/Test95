using VAA.EAPI.Microservices.Loyalty.Identity.Validation;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class ValidateEmailTests
    {
        [Fact]
        public void ValidateEmail_ShouldReturnTrue_ForValidEmail()
        {
            //Arrange
            var request = "Test@fly.virgin.com";

            //Act
            var result = ValidateEmail.Validate(request);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateEmail_ShouldReturnfalse_ForInValidEmail()
        {
            //Arrange
            var request = "Test@fly@virgin.com";

            //Act
            var result = ValidateEmail.Validate(request);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateEmail_ShouldReturnfalse_ForNullEmail()
        {
            //Arrange
            //Act
            var result = ValidateEmail.Validate(null);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateEmail_ShouldReturnfalse_ForEmptyEmail()
        {
            //Arrange
            //Act
            var result = ValidateEmail.Validate(string.Empty);

            //Assert
            Assert.False(result);
        }
    }
}
