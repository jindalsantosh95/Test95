using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class UpdateAzureB2CProfileHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IServiceProvider _serviceProvider;

        public UpdateAzureB2CProfileHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;

            IdentityTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<UnVerifiedMemberValidationServiceHandler>()
                .AddSingleton<UpdateB2CServiceCommandHandler>()
                .AddSingleton<UpdateAzureB2CProfileCommandHandler>()
                .AddSingleton<UpdateAzureB2CProfileRepoCommandHandler>()
                .AddSingleton<IUpdateAzureB2CProfileRepoCommandHandler, UpdateAzureB2CProfileRepoCommandHandler>()
                .BuildServiceProvider();

        }

        private UpdateAzureB2CProfileCommandHandler ResolveHandler()
            => _serviceProvider.GetRequiredService<UpdateAzureB2CProfileCommandHandler>();

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForInvalidEmail()
        {
            var handler = ResolveHandler();

            var request = TestDataProvider.GetTestData<UpdateAzureB2CProfileCommand>(
                "IdentityMS\\B2CProfileUpdateInValidRequest"
            );

            var updateData = Json.DeserializeObject<UpdateAzureB2CProfileRequest>((JToken)request.Data);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.False(result.Value);
            Assert.False(ValidateEmail.Validate(updateData.NewEmailId));
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_WhenRequiredFieldsMissing()
        {
            var handler = ResolveHandler();

            var request = new UpdateAzureB2CProfileCommand
            {
                Data = JObject.FromObject(new
                {
                    MemberNumber = "",
                    FirstName = "",
                    LastName = ""
                })
            };

            var updateData = Json.DeserializeObject<UpdateAzureB2CProfileRequest>((JToken)request.Data);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.False(result.Value);
            Assert.True(string.IsNullOrWhiteSpace(updateData.MemberNumber));
            Assert.True(string.IsNullOrWhiteSpace(updateData.FirstName));
            Assert.True(string.IsNullOrWhiteSpace(updateData.LastName));
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenMemberIsValid()
        {

            var repoHandler = _serviceProvider.GetRequiredService<UpdateAzureB2CProfileRepoCommandHandler>();

            var request = TestDataProvider.GetTestData<UpdateAzureB2CProfileRepoCommand>(
                "IdentityMS\\B2CProfileUpdateEventRequest");

            var result = await repoHandler.Handle(request, CancellationToken.None);

            Assert.True(result);

            var responses = _httpClientFactoryFixture.Current.GetResponses();

            var validationRequest = responses.FirstOrDefault(r => r.RequestMessage!.RequestUri!.ToString().Is("https://tempuri.org/v1.0/users/?$select=Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled,extension_b4e6641f44c644ea8b9a899e786c7579_MemberStatus&$filter=Identities/any(x:x/issuer eq 'TestDomain' and issuerAssignedId eq '15008960612')"));

            Assert.NotNull(validationRequest);


            var updateRequest = responses.FirstOrDefault(r => r.RequestMessage!.RequestUri!.ToString().Is("https://tempuri.org/beta/users/84d4dda6-368f-4575-9470-0c84a45be868"));

            Assert.NotNull(updateRequest);

            var json = await validationRequest.Content.ReadAsStringAsync();

            var root = JObject.Parse(json);

            var validationResponse = root["value"]![0]!.ToObject<Value>();

            Assert.NotNull(validationResponse);
            Assert.Contains(validationResponse.Identities, i => i.IssuerAssignedId == request.updateAzureB2CProfileRequest.NewEmailId);
            Assert.Equal(validationResponse.GivenName, request.updateAzureB2CProfileRequest.FirstName);
            Assert.Equal(validationResponse.Surname, request.updateAzureB2CProfileRequest.LastName);
            Assert.Equal(validationResponse.AdditionalData["extension_b4e6641f44c644ea8b9a899e786c7579_MemberStatus"]!.ToString(), request.updateAzureB2CProfileRequest.AccountStatus);
        }

    }
}

