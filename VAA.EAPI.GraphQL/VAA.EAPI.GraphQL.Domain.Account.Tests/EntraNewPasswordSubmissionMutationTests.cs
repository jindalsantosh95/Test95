using HotChocolate;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class EntraNewPasswordSubmissionMutationTests
    {
        private readonly Mock<IEntraServicesProvider<SignInResponse>> _entraNewPassword;
        private readonly Mock<ILogger<EntraNewPasswordSubmissionMutation>> _logger;
        private readonly EntraNewPasswordSubmissionMutation _mutation;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public EntraNewPasswordSubmissionMutationTests()
        {
            _entraNewPassword = new Mock<IEntraServicesProvider<SignInResponse>>();
            _logger = new Mock<ILogger<EntraNewPasswordSubmissionMutation>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mutation = new EntraNewPasswordSubmissionMutation(_httpConnector.Object, _tokenProvider.Object, _entraNewPassword.Object, _logger.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraNewPasswordSubmissionMutation(null, _tokenProvider.Object, _entraNewPassword.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraNewPasswordSubmissionMutation(_httpConnector.Object, null, _entraNewPassword.Object, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenEntraNewPasswordObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraNewPasswordSubmissionMutation(_httpConnector.Object, _tokenProvider.Object, null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraNewPasswordSubmissionMutation(_httpConnector.Object, _tokenProvider.Object, _entraNewPassword.Object, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowNullException_WhenTokenisNull()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            newPasswordSubmissionRequest.NewPassword = null;

            //Act //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.EntraNewPassword(newPasswordSubmissionRequest));
        }

        [Fact]
        public async Task Mutation_ShouldThrowNullTokenValue_WhenOtpValueisNull()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            newPasswordSubmissionRequest.Token = null;

            //Act //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.EntraNewPassword(newPasswordSubmissionRequest));
        }

        [Fact]
        public async Task Mutation_Shouldreturn_Id_Access_Refresh_Token_ForGoodRequest()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            var passwordVerifcationResponse = TestDataProvider.GetTestData<IEnumerable<PasswordPolicy>>("Account//PasswordPoliciesResponse");
            var signInResponse = TestDataProvider.GetTestData<SignInResponse>("Account//RefreshTokenResponse");

            //Act
            _httpConnector.Setup(c => c.SendAsync<IEnumerable<PasswordPolicy>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new APIResponse<IEnumerable<PasswordPolicy>>
            {
                Response = passwordVerifcationResponse
            });

            _entraNewPassword.Setup(x => x.NewPasswordSubmission(It.IsAny<NewPasswordSubmissionRequest>())).ReturnsAsync(signInResponse);

            var result = await _mutation.EntraNewPassword(newPasswordSubmissionRequest);

            //Assert
            Assert.NotNull(result.IdToken);
            Assert.IsType<SignInResponse>(result);
            Assert.IsType<string>(result.AccessToken);
            Assert.IsType<int>(result.ExpiresIn);
            Assert.StartsWith("1.ATI", result.RefreshToken);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.IdToken));
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.AccessToken));
        }

        [Fact]
        public void EntraPasswordSubmissionRequest_ShouldMapPasswordSubmissionFields()
        {
            var request = new NewPasswordSubmissionRequest
            {
                Email = "test@example.com",
                Token = "continuation-token",
                NewPassword = "Virgin123"
            };

            var result = (EntraPasswordSubmissionRequest)request;

            Assert.Equal("continuation-token", result.ContinuationToken);
            Assert.Equal("Virgin123", result.Password);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForExpiredTokeninRequest()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            var passwordVerifcationResponse = TestDataProvider.GetTestData<IEnumerable<PasswordPolicy>>("Account//PasswordPoliciesResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//ContinuatoinTokenExpiredResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

            //Act
            _httpConnector.Setup(c => c.SendAsync<IEnumerable<PasswordPolicy>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new APIResponse<IEnumerable<PasswordPolicy>>
            {
                Response = passwordVerifcationResponse
            });

            _entraNewPassword.Setup(c => c.NewPasswordSubmission(It.IsAny<NewPasswordSubmissionRequest>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraNewPassword(newPasswordSubmissionRequest));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForInvlaidPasswordcharacter()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            newPasswordSubmissionRequest.NewPassword = "Virgin123@";

            var passwordVerifcationResponse = TestDataProvider.GetTestData<IEnumerable<PasswordPolicy>>("Account//PasswordPoliciesResponse");
            passwordVerifcationResponse.First(x => x.PolicyDescription == "Must not contain the '@' symbol").IsValid = false;

            //Act
            _httpConnector.Setup(c => c.SendAsync<IEnumerable<PasswordPolicy>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new APIResponse<IEnumerable<PasswordPolicy>>
            {
                Response = passwordVerifcationResponse
            });

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraNewPassword(newPasswordSubmissionRequest));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForPasswordwithNoNumber()
        {
            //Arrange
            var newPasswordSubmissionRequest = TestDataProvider.GetTestData<NewPasswordSubmissionRequest>("Account//EntraPasswordSubmissionRequest");
            newPasswordSubmissionRequest.NewPassword = "VirginAtlantic";

            var passwordVerifcationResponse = TestDataProvider.GetTestData<IEnumerable<PasswordPolicy>>("Account//PasswordPoliciesResponse");
            passwordVerifcationResponse.First(x => x.PolicyDescription == "Must contain at least 1 number").IsValid = false;

            //Act
            _httpConnector.Setup(c => c.SendAsync<IEnumerable<PasswordPolicy>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new APIResponse<IEnumerable<PasswordPolicy>>
            {
                Response = passwordVerifcationResponse
            });

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraNewPassword(newPasswordSubmissionRequest));
        }
    }
}
