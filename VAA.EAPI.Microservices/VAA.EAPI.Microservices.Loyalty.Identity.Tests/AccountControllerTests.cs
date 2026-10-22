using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.HTTP;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class AccountControllerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountControllerTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<Core.Xunit.Http.HttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ITableStorageService<GenerateOtpDbEntities>, TableStorageService<GenerateOtpDbEntities>>()
                .AddSingleton<IGetAdUserDetailsService, GetAdUserDetailsService>()
                .AddSingleton<IVerifyMemberStatusRepository, VerifyMemberStatusRepository>()
                .AddSingleton<IEntraCustomAuthExtensionsService, EntraCustomAuthExtensionsService>()
                .AddSingleton<IOtpTokenValidationService, OtpTokenValidationService>()
                .AddSingleton<IOTPGenerateCommandHandler, TestOTPGenerateCommandHandler>()
                .AddSingleton<IOTPGenerateContinuationTokenCommandHandler, TestOTPGenerateContinuationTokenCommandHandler>()
                .AddSingleton<TestRegisterYoungFlyerCommandHandler>()
                .AddSingleton<IRegisterYoungFlyerCommandHandler>(sp => sp.GetRequiredService<TestRegisterYoungFlyerCommandHandler>())
                .AddSingleton<AccountController>()
                .AddMediatR(config => config.RegisterServicesFromAssemblyContaining<OTPGenerateCommand>())
                .BuildServiceProvider();
        }

        [Fact]
        public async Task OtpTokenValidation_ReturnsEmailAddress_WhenTokenIsValid()
        {
            var token = await ValidationTokenHelper.CreateToken("test@test.com");
            var output = await ResolveController().OtpTokenValidation(CreateRequest($$"""{"token":"{{token}}"}"""));

            var result = await ReadResponseData<string>(output);

            Assert.Equal(HttpStatusCode.OK, output.StatusCode);
            Assert.Equal("test@test.com", result);
        }

        [Fact]
        public async Task OtpTokenValidation_ReturnsError_WhenTokenIsInvalid()
        {
            var output = await ResolveController().OtpTokenValidation(CreateRequest("""{"token":"TestToken"}"""));

            var result = await ReadResponseData<List<ErrorDetails>>(output);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, output.StatusCode);
            Assert.Equal("10199", result.First().Code);
            Assert.Contains("Enrolment Token Validation Failed due to Invalid or Expired Token.", result.First().Message);
        }

        [Fact]
        public async Task RegisterYoungFlyer_DisablesTheAccountAndSuppressesCommunication()
        {
            var request = """{"customerDetails":{}}""";

            var output = await ResolveController().RegisterYoungFlyer(CreateRequest(request), new TestFunctionContext());

            Assert.Equal(HttpStatusCode.OK, output.StatusCode);
            var handler = _serviceProvider.GetRequiredService<TestRegisterYoungFlyerCommandHandler>();
            Assert.NotNull(handler.Request);
            Assert.False(handler.Request.AccountEnabled);
            Assert.True(handler.Request.SuppressCommunication);
        }

        private AccountController ResolveController()
            => _serviceProvider.GetRequiredService<AccountController>();

        private static HttpRequestData CreateRequest(string body)
        {
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return new TestHttpRequestData(
                new TestFunctionContext(),
                new Uri("https://tempuri.org/account"),
                bodyStream,
                "POST");
        }

        private static async Task<T> ReadResponseData<T>(HttpResponseData responseData)
        {
            responseData.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseData.Body);
            var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            return Json.Deserialize<T>(responseBody);
        }

        private sealed class TestOTPGenerateCommandHandler : IOTPGenerateCommandHandler
        {
            public Task<Response<string>> Handle(OTPGenerateCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Response<string>
                {
                    Status = Status.OK,
                    Value = string.Empty
                });
            }
        }

        private sealed class TestOTPGenerateContinuationTokenCommandHandler : IOTPGenerateContinuationTokenCommandHandler
        {
            public Task<Response<string>> Handle(OTPGenerateContinuationTokenCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Response<string>
                {
                    Status = Status.OK,
                    Value = string.Empty
                });
            }
        }

        private sealed class TestRegisterYoungFlyerCommandHandler : IRegisterYoungFlyerCommandHandler
        {
            public RegisterYoungFlyerRequest Request { get; private set; }

            public Task<Response<B2CUserResponse>> Handle(RegisterYoungFlyerRequest request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(new Response<B2CUserResponse>
                {
                    Status = Status.OK,
                    Value = new B2CUserResponse { IsB2COperationSuccess = true }
                });
            }
        }
    }
}
