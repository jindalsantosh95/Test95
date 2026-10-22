using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class StartSignUpHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;       

        public StartSignUpHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
                    
            _serviceProvider = new ServiceCollection()
               .AddLogging()               
               .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
               .AddSingleton<IMediator>(serviceProvider => new Mediator(serviceProvider))
               .AddSingleton<IHttpConnector, HttpConnector>()               
               .AddSingleton<IEntraService, EntraService>()
               .AddSingleton<StartSignUpHandler>()
               .BuildServiceProvider();
        }

        private static void SetEntraApiUrlDomain(string value)
           => ConfigReader.Set(Identity.Constants.Environment.EntraAPIurlDomain, value);

        [Fact]
        public async Task StartSignUp_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;
            var result = await handler.StartSignUp(null, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(ErrorCodes.BadRequest, result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Request is null", result.Errors.First().Message);
        }

        [Fact]
        public async Task StartSignUp_ShouldReturnBadRequest_WhenUsernameIsMissing()
        {
            var request = new AccountSignUpRequest { Username = "   " };
            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;

            var result = await handler.StartSignUp(request, CancellationToken.None);            

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(ErrorCodes.BadRequest, result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Username is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task StartSignUp_ShouldReturnBusinessError_WhenStartReturnsErrorJson()
        {    
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/error");

            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;
            var result = await handler.StartSignUp(new AccountSignUpRequest { Username = "test@example.com" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("1003037", result.Errors.First().Code);
            Assert.Equal("user_already_exists", result.Errors.First().StatusText);
            Assert.StartsWith("AADSTS1003037: It looks like you may already have an account with us using this email address.", result.Errors.First().Message);
        }

        [Fact]
        public async Task StartSignUp_ShouldReturnTechnicalError_WhenContinuationTokenIsMissing()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/missing");

            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;
            var result = await handler.StartSignUp(new AccountSignUpRequest { Username = "test@example.com" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(ErrorCodes.MiscellaneousTechnicalError, result.Errors.First().Code);
            Assert.Equal("invalid_response", result.Errors.First().StatusText);
            Assert.Equal("Invalid sign-up response: missing continuation token", result.Errors.First().Message);
        }

        [Fact]
        public async Task StartSignUp_ShouldReturnContinuationToken_WhenFlowSucceeds()
        {                        
            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;
            var result = await handler.StartSignUp(new AccountSignUpRequest { Username = "test@example.com" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.StartsWith("X0ZwcEM5ak9WWDB1WkNFcnYtcVlVYXR5UkpyVTA3di05NnZOVTRaMEdqVlElM2QlM2Q", result.Value);
            Assert.Null(result.Errors);           
        }

        [Fact]
        public async Task StartSignUp_ShouldReturnUnexpectedError_WhenExceptionIsThrown()
        {
            SetEntraApiUrlDomain("invalid domain");

            var handler = _serviceProvider.GetService<StartSignUpHandler>()!;
            var result = await handler.StartSignUp(new AccountSignUpRequest { Username = "test@example.com" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(ErrorCodes.MiscellaneousTechnicalError, result.Errors.First().Code);
            Assert.Equal("unexpected_error", result.Errors.First().StatusText);
            Assert.Equal("An unexpected error occurred during registration", result.Errors.First().Message);
        }
    }
}
