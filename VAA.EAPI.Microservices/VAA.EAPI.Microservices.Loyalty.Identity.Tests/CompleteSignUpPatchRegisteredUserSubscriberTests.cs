using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class CompleteSignUpPatchRegisteredUserSubscriberTests
    {
        private readonly Core.Xunit.Http.HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public CompleteSignUpPatchRegisteredUserSubscriberTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<Core.Xunit.Http.HttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IGetAdUserDetailsService, GetAdUserDetailsService>()
                .AddSingleton<IVerifyMemberStatusRepository, VerifyMemberStatusRepository>()
                .AddSingleton<IUpdateAdUserService, UpdateAdUserService>()
                .AddSingleton<IPatchRegisteredUserHandler, PatchRegisteredUserHandler>()
                .AddSingleton<CompleteSignUpPatchRegisteredUserSubscriber>()
                .BuildServiceProvider();

            _httpClientFactory = _serviceProvider.GetRequiredService<Core.Xunit.Http.HttpClientFactory>();
        }

        [Fact]
        public async Task Run_GoodRequest_ShouldPatchRegisteredUser()
        {
            // Arrange
            var subscriber = _serviceProvider.GetRequiredService<CompleteSignUpPatchRegisteredUserSubscriber>();
            var cloudEvent = CreatePatchRegisteredUserEvent();
            var responseCountsBefore = CaptureResponseCounts();

            // Act
            await subscriber.Run(Json.Serialize(cloudEvent, false), TestContext.Current.CancellationToken);

            // Assert
            var newHttpResponses = GetNewHttpResponses(responseCountsBefore);
            var verifyUserResponse = newHttpResponses.FirstOrDefault(response =>
                response.RequestMessage!.RequestUri!.ToString().Contains("jane.doe%40example.com"));

            Assert.NotNull(verifyUserResponse);

            var patchResponse = newHttpResponses.FirstOrDefault(response =>
                response.RequestMessage!.Method == HttpMethod.Patch &&
                response.RequestMessage!.RequestUri!.ToString() == "https://tempuri.org/v1.0/users/84d4dda6-368f-4575-9470-0c84a45be868");

            Assert.NotNull(patchResponse);

            var patchContent = await patchResponse.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var patchJson = JObject.Parse(patchContent);
            var identities = (JArray)patchJson[ConfigReader.Get(Constants.Environment.Identities)]!;

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "emailAddress" &&
                identity["issuerAssignedId"]?.ToString() == "jane.doe@example.com");

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "username" &&
                identity["issuerAssignedId"]?.ToString() == "1234567890");

            Assert.Equal("Individual", patchJson[$"{ConfigReader.Get(Constants.Environment.ExtensionPrefix)}{ConfigReader.Get(Constants.Environment.CustomMemberTypeAttribute)}"]?.ToString());
            Assert.Equal("ACTIVE", patchJson[$"{ConfigReader.Get(Constants.Environment.ExtensionPrefix)}{ConfigReader.Get(Constants.Environment.CustomMemberStatusAttribute)}"]?.ToString());
            Assert.Equal("customer-1", patchJson[$"{ConfigReader.Get(Constants.Environment.ExtensionPrefix)}{ConfigReader.Get(Constants.Environment.CustomCustomerIdAttribute)}"]?.ToString());
            Assert.Null(patchJson[ConfigReader.Get(Constants.Environment.PasswordProfile)]);
            Assert.Null(patchJson[ConfigReader.Get(Constants.Environment.UserPrincipalName)]);
        }

        [Fact]
        public async Task Run_MissingEventData_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriber = _serviceProvider.GetRequiredService<CompleteSignUpPatchRegisteredUserSubscriber>();
            var cloudEvent = CreatePatchRegisteredUserEvent();
            cloudEvent.Data = null;
            var responseCountsBefore = CaptureResponseCounts();

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                subscriber.Run(Json.Serialize(cloudEvent, false), TestContext.Current.CancellationToken));

            // Assert
            Assert.Equal("Patch registered user event data is missing.", exception.Message);
            Assert.Empty(GetNewHttpResponses(responseCountsBefore));
        }

        private IReadOnlyDictionary<string, int> CaptureResponseCounts()
        {
            var entraBaseUrl = $"https://{ConfigReader.Get(Constants.Environment.EntraAPIurlDomain)}";
            var graphBaseUrl = ConfigReader.Get(Constants.Environment.GraphAPIUrl);

            return new Dictionary<string, int>
            {
                [entraBaseUrl] = _httpClientFactory.GetResponses(entraBaseUrl).Count,
                [graphBaseUrl] = _httpClientFactory.GetResponses(graphBaseUrl).Count
            };
        }

        private List<HttpResponseMessage> GetNewHttpResponses(IReadOnlyDictionary<string, int> responseCountsBefore)
        {
            var entraBaseUrl = $"https://{ConfigReader.Get(Constants.Environment.EntraAPIurlDomain)}";
            var graphBaseUrl = ConfigReader.Get(Constants.Environment.GraphAPIUrl);

            return _httpClientFactory.GetResponses(entraBaseUrl)
                .Skip(responseCountsBefore[entraBaseUrl])
                .Concat(_httpClientFactory.GetResponses(graphBaseUrl).Skip(responseCountsBefore[graphBaseUrl]))
                .ToList();
        }

        private static CloudEvent CreatePatchRegisteredUserEvent()
        {
            return new CloudEvent
            {
                Id = "event-1",
                Type = Constants.Application.PatchRegisteredUserEventType,
                Subject = Constants.Application.PatchRegisteredUserEventSubject,
                Source = Constants.Application.IdentityEventHandlerTopicName,
                Data = new PatchRegisteredUserRequest
                {
                    MemberNumber = "1234567890",
                    EmailAddress = "JANE.DOE@EXAMPLE.COM",
                    FirstName = "Jane",
                    LastName = "Doe",
                    CustomerId = "customer-1",
                    CountryCode = "GB",
                    MemberType = "Individual"
                }
            };
        }
    }
}
