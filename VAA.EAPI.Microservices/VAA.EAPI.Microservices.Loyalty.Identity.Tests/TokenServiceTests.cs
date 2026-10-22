using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Security;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class TokenServiceTests
    {
        [Fact(Skip = "Not sure the value in this test as how we are storing claims in incorrect")]
        public void ShouldGenerateToken_WithValidPNRInClaims()
        {
            //Arrange 
            var request = new List<Claim>() { new Claim() { RLOC = "dummy" } };
            ConfigReader.Set("IdentityAcquireToken:Secret", "dummySecretToGenerateTokenaaaaaa");
            ConfigReader.Set("IdentityAcquireToken:ExpiryMinutes", "20");

            //Act
            var service = new TokenService<Claim>();
            var result = service.GenerateAccessToken(request);

            //Assert
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result);

            Assert.Contains("dummy", token.Claims.First(x => x.Type == "Claim").Value);
        }
    }
}
