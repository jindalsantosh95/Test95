using HotChocolate;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class RefreshTokenMutationTests
    {
        private readonly Mock<IEntraServicesProvider<SignInResponse>> _entraServicesRefreshToken;
        private readonly Mock<ILogger<RefreshTokenMutation>> _logger;
        private readonly RefreshTokenMutation _mutation;

        public RefreshTokenMutationTests()
        {
            _entraServicesRefreshToken = new Mock<IEntraServicesProvider<SignInResponse>>();
            _logger = new Mock<ILogger<RefreshTokenMutation>>();
            _mutation = new RefreshTokenMutation(_entraServicesRefreshToken.Object, _logger.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshTokenMutation(_entraServicesRefreshToken.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenProductRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RefreshTokenMutation(null, _logger.Object));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullRequest()
        {
            //Arrange
            var refreshTokenRequest = TestDataProvider.GetTestData<RefreshTokenRequest>("Account//RefreshTokenRequest");
            refreshTokenRequest.Token = null;

            //Act
            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.RefreshTokens(refreshTokenRequest));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForEmptyRequest()
        {
            //Arrange
            var refreshTokenRequest = TestDataProvider.GetTestData<RefreshTokenRequest>("Account//RefreshTokenRequest");
            refreshTokenRequest.Token = string.Empty;

            //Act
            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.RefreshTokens(refreshTokenRequest));
        }

        [Fact]
        public async Task Mutation_ShouldReturnAccessToken_ForGoodRequest()
        {
            //Arrange
            var refreshTokenRequest = TestDataProvider.GetTestData<RefreshTokenRequest>("Account//RefreshTokenRequest");
            var refreshTokenResponse = TestDataProvider.GetTestData<SignInResponse>("Account//RefreshTokenResponse");

            //Act
            _entraServicesRefreshToken.Setup(x => x.GenerateRefreshToken(It.IsAny<RefreshTokenRequest>())).ReturnsAsync(refreshTokenResponse);

            var result = await _mutation.RefreshTokens(refreshTokenRequest);

            //Assert
            Assert.NotNull(result.AccessToken);
            Assert.IsType<SignInResponse>(result);
            Assert.Equal("Bearer", result.TokenType);
            Assert.NotNull(result.RefreshToken);
            Assert.IsType<int>(result.ExpiresIn);
        }

        [Fact]
        public async Task Mutation_ShouldThorwError_ForWInvalidRefreshToken()
        {
            //Arrange
            var refreshTokenRequest = TestDataProvider.GetTestData<RefreshTokenRequest>("Account//RefreshTokenRequest");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//RefreshTokenErrorResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

            //Act
            _entraServicesRefreshToken.Setup(x => x.GenerateRefreshToken(It.IsAny<RefreshTokenRequest>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));


            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.RefreshTokens(refreshTokenRequest));
        }
    }
}
