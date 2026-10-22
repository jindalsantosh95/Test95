using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using InfrastructureConstants = VAA.EAPI.Infrastructure.Constants.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class CompleteSignUpHandlerTests
    {
        private readonly Core.Xunit.Http.HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public CompleteSignUpHandlerTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set(InfrastructureConstants.MicroServiceDBKey, "test-db-key");
            ConfigReader.Set(InfrastructureConstants.MicroServiceEventStoreTableName, "test-event-store");
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-connection-string");

            _serviceProvider = new ServiceCollection()
               .AddLogging()
               .AddSingleton<Core.Xunit.Http.HttpClientFactory>()
               .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<Core.Xunit.Http.HttpClientFactory>())
               .AddSingleton<IHttpConnector, HttpConnector>()
               .AddSingleton<IEntraService, EntraService>()
               .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
               .AddSingleton<IMessageBusFactory, MessageBusFactory>()
               .AddSingleton<EventStoreRequestInsertCommandHandler>()
               .AddSingleton<EventStoreRequestDeleteCommandHandler>()
               .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
               .AddSingleton<IServiceBusTopicMessageEventCommandHandler>(sp => sp.GetRequiredService<ServiceBusTopicMessageEventCommandHandler>())
               .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>()
               .AddSingleton<IGetAdUserDetailsService, GetAdUserDetailsService>()
               .AddSingleton<IVerifyMemberStatusRepository, VerifyMemberStatusRepository>()
               .AddSingleton<IUpdateAdUserService, UpdateAdUserService>()
               .AddSingleton<IPatchRegisteredUserHandler, PatchRegisteredUserHandler>()
               .AddSingleton<CompleteSignUpHandler>()
               .BuildServiceProvider();

            _httpClientFactory = _serviceProvider.GetRequiredService<Core.Xunit.Http.HttpClientFactory>();
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;

            var result = await handler.CompleteSignUp(null, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Request is null", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenGenderIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();
            
            request.Gender = null; // Remove gender to simulate missing gender

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Gender is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenLastNameIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.LastName = null; 

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Last Name is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenFirstNameIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.FirstName = null; 

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("First Name is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenDOBIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.DateOfBirth = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Date of Birth is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenCustomerIdIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.CustomerId = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Customer Id is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenContinuationTokenIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.ContinuationToken = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Continuation token is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenPasswordIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.Password = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Password is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenMemberNumberIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.FcNumber = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Member number is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenEmailDetailsIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.ContactDetails!.EmailDetails = new List<AccountEmailDetail>();

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Email address is required", result.Errors.First().Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenEmailAddressIsMissingOrEmpty(string? emailAddress)
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.ContactDetails!.EmailDetails = new List<AccountEmailDetail>
            {
                new AccountEmailDetail
                {
                    EmailId = emailAddress,
                    PreferredFlag = "Y"
                }
            };

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Email address is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenLoyaltyMemberIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.LoyaltyMember = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Loyalty member details are required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenAddressDetailsIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.AddressDetails = null;

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("At least one address detail is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenAddressDetailsCountryIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.AddressDetails = new List<AccountAddressDetail>(){
                    new AccountAddressDetail
                    {
                        AddressLine1 = "1 Test Street",
                        City = "London",
                        Country = null,
                        PostalCode = "SW1A-1AA",
                        PreferredFlag = "Y"
                    }
                };

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Address country code is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBadRequest_WhenAddressEnrolmentChannelIsMissing()
        {
            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();

            request.LoyaltyMember.EnrollmentChannel = null;
                  
            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Enrolment channel is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnTokenAndNormalizeRequest_WhenFlowSucceeds()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com");

            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var request = CreateValidRequest();
            var responseCountsBefore = CaptureResponseCounts();
            var publishedEvents = CapturePublishedEvents();

            var result = await handler.CompleteSignUp(request, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjJa", result.Value.AccessToken);
            Assert.StartsWith("0.AV8AWyS-BgKh8kmG/OTK9n8EegMAAAAAAAAAwAAAAAAAAAAzAAA", result.Value.RefreshToken);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjJa", result.Value.IdToken);
            Assert.Equal(3600, result.Value.ExpiresIn);
            Assert.Equal("Bearer", result.Value.TokenType);

            var newHttpResponses = GetNewHttpResponses(responseCountsBefore);
            var continueRequests = newHttpResponses
                .Where(response => response.RequestMessage!.RequestUri!.ToString().Contains("/signup/v1.0/continue"))
                .ToList();

            Assert.Single(continueRequests);

            var continueBody = await continueRequests.Single().RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("grant_type=password", continueBody);
            Assert.DoesNotContain("grant_type=attributes", continueBody);
            Assert.DoesNotContain("attributes=", continueBody);

            Assert.DoesNotContain(newHttpResponses, response =>
                response.RequestMessage!.RequestUri!.ToString().StartsWith("https://tempuri.org/v1.0/users/"));

            var patchEvent = Assert.Single(publishedEvents, cloudEvent => cloudEvent.Type == Constants.Application.PatchRegisteredUserEventType);
            Assert.Equal(Constants.Application.PatchRegisteredUserEventSubject, patchEvent.Subject);
            Assert.Equal(Constants.Application.IdentityEventHandlerTopicName, patchEvent.Source);

            var patchRequest = Assert.IsType<PatchRegisteredUserRequest>(patchEvent.Data);

            Assert.Equal("1234567890", patchRequest.MemberNumber);
            Assert.Equal("jane.doe@example.com", patchRequest.EmailAddress);
            Assert.Equal("Jane", patchRequest.FirstName);
            Assert.Equal("Doe", patchRequest.LastName);
            Assert.Equal("customer-1", patchRequest.CustomerId);
            Assert.Equal("GB", patchRequest.CountryCode);
            Assert.Equal("Individual", patchRequest.MemberType);
            Assert.DoesNotContain("password", Json.Serialize(patchEvent.Data), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PatchRegisteredUserHandler_ShouldPatchRegisteredUser_WhenGraphReturnsExistingIdentities()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchRegisteredUserHandler>();
            var request = CreatePatchRegisteredUserRequest("1234567890", "JANE.DOE@EXAMPLE.COM");
            var responseCountsBefore = CaptureResponseCounts();

            await handler.Handle(request, TestContext.Current.CancellationToken);

            var newHttpResponses = GetNewHttpResponses(responseCountsBefore);
            var verifyUserRequest = newHttpResponses.FirstOrDefault(response =>
                response.RequestMessage!.RequestUri!.ToString().Contains("jane.doe%40example.com"));

            Assert.NotNull(verifyUserRequest);

            var patchResponse = newHttpResponses.FirstOrDefault(response =>
                response.RequestMessage!.Method == HttpMethod.Patch &&
                response.RequestMessage!.RequestUri!.ToString() == "https://tempuri.org/v1.0/users/84d4dda6-368f-4575-9470-0c84a45be868");

            Assert.NotNull(patchResponse);

            var patchContent = await patchResponse.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var patchJson = JObject.Parse(patchContent);
            var identities = (JArray)patchJson["identities"]!;

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "emailAddress" &&
                identity["issuerAssignedId"]?.ToString() == "jane.doe@example.com");

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "username" &&
                identity["issuerAssignedId"]?.ToString() == "1234567890");

            Assert.Equal("Individual", patchJson["extension_b4e6641f44c644ea8b9a899e786c7579_MemberType"]?.ToString());
            Assert.Equal("ACTIVE", patchJson["extension_b4e6641f44c644ea8b9a899e786c7579_memberStatus"]?.ToString());
            Assert.Equal("customer-1", patchJson["extension_b4e6641f44c644ea8b9a899e786c7579_CustomerId"]?.ToString());
            Assert.Null(patchJson["passwordProfile"]);
            Assert.Null(patchJson["userPrincipalName"]);
        }

        [Fact]
        public async Task PatchRegisteredUserHandler_ShouldUseRequestedIdentities_WhenGraphReturnsNoExistingIdentities()
        {
            var handler = _serviceProvider.GetRequiredService<IPatchRegisteredUserHandler>();
            var request = CreatePatchRegisteredUserRequest("0987654321", "MISSING.IDENTITIES@EXAMPLE.COM");
            var responseCountsBefore = CaptureResponseCounts();

            await handler.Handle(request, TestContext.Current.CancellationToken);

            var newHttpResponses = GetNewHttpResponses(responseCountsBefore);
            var patchResponse = newHttpResponses.FirstOrDefault(response =>
                response.RequestMessage!.Method == HttpMethod.Patch &&
                response.RequestMessage!.RequestUri!.ToString() == "https://tempuri.org/v1.0/users/fallback-identities-user-id");

            Assert.NotNull(patchResponse);

            var patchContent = await patchResponse.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var patchJson = JObject.Parse(patchContent);
            var identities = (JArray)patchJson["identities"]!;

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "emailAddress" &&
                identity["issuerAssignedId"]?.ToString() == "missing.identities@example.com");

            Assert.Contains(identities, identity =>
                identity["signInType"]?.ToString() == "username" &&
                identity["issuerAssignedId"]?.ToString() == "0987654321");
        }

        private List<CloudEvent> CapturePublishedEvents()
        {
            var publishedEvents = new List<CloudEvent>();
            var messageBusFactory = _serviceProvider.GetRequiredService<IMessageBusFactory>();
            var messageBus = (MessageBus)messageBusFactory.GetClient(
                ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                Constants.Application.IdentityEventHandlerTopicName);

            messageBus.OnMessage += publishedEvents.Add;

            return publishedEvents;
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

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBusinessException_WhenChallengeEndpointReturnsError()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/InvalidChallenge");

            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var result = await handler.CompleteSignUp(CreateValidRequest(), TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("901007", result.Errors.First().Code);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBusinessException_WhenChallengeEndpointReturnsRedirectResponse()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/redirect");

            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var result = await handler.CompleteSignUp(CreateValidRequest(), TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("Authentication requires redirect to web-based flow", result.Errors.First().Message);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnBusinessException_WhenChallengeContinuationTokenMissing()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/missingToken");

            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;

            var result = await handler.CompleteSignUp(CreateValidRequest(), TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("G009", result.Errors.First().Code);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldReturnTokenResponse_WhenTokenEndpointReturnsErrorAndTokens()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/tokenWithError");

            var handler = _serviceProvider.GetService<CompleteSignUpHandler>()!;
            var result = await handler.CompleteSignUp(CreateValidRequest(), TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjJa", result.Value.AccessToken);
            Assert.StartsWith("0.AV8AWyS-BgKh8kmG/OTK9n8EegMAAAAAAAAAwAAAAAAAAAAzAAA", result.Value.RefreshToken);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjJa", result.Value.IdToken);
            Assert.Equal("Bearer", result.Value.TokenType);
        }

        [Fact]
        public async Task CompleteSignUp_ShouldCompletePreverifiedMfaRegistration_WhenTokenEndpointRequiresRegistration()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/mfaRegistration");

            var handler = _serviceProvider.GetRequiredService<CompleteSignUpHandler>();
            var responseCountsBefore = CaptureResponseCounts();

            var result = await handler.CompleteSignUp(CreateValidRequest(), TestContext.Current.CancellationToken);

            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal("registration-access-token", result.Value.AccessToken);
            Assert.Equal("registration-refresh-token", result.Value.RefreshToken);
            Assert.Equal("registration-id-token", result.Value.IdToken);

            var newHttpResponses = GetNewHttpResponses(responseCountsBefore);
            Assert.Contains(newHttpResponses, response => response.RequestMessage!.RequestUri!.AbsolutePath.EndsWith("/register/v1.0/introspect", StringComparison.Ordinal));
            Assert.Contains(newHttpResponses, response => response.RequestMessage!.RequestUri!.AbsolutePath.EndsWith("/register/v1.0/challenge", StringComparison.Ordinal));

            var registrationContinueRequest = Assert.Single(newHttpResponses, response => response.RequestMessage!.RequestUri!.AbsolutePath.EndsWith("/register/v1.0/continue", StringComparison.Ordinal));
            var registrationContinueBody = await registrationContinueRequest.RequestMessage!.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("grant_type=continuation_token", registrationContinueBody);
            Assert.DoesNotContain("oob=", registrationContinueBody, StringComparison.OrdinalIgnoreCase);

            var registrationFlowPaths = newHttpResponses
                .Select(response => response.RequestMessage!.RequestUri!.AbsolutePath)
                .Where(path =>
                    path.EndsWith("/oauth2/v2.0/token", StringComparison.Ordinal) ||
                    path.EndsWith("/register/v1.0/introspect", StringComparison.Ordinal) ||
                    path.EndsWith("/register/v1.0/challenge", StringComparison.Ordinal) ||
                    path.EndsWith("/register/v1.0/continue", StringComparison.Ordinal))
                .ToList();

            Assert.Collection(
                registrationFlowPaths,
                path => Assert.EndsWith("/oauth2/v2.0/token", path, StringComparison.Ordinal),
                path => Assert.EndsWith("/register/v1.0/introspect", path, StringComparison.Ordinal),
                path => Assert.EndsWith("/register/v1.0/challenge", path, StringComparison.Ordinal),
                path => Assert.EndsWith("/register/v1.0/continue", path, StringComparison.Ordinal),
                path => Assert.EndsWith("/oauth2/v2.0/token", path, StringComparison.Ordinal));
        }
        
        private static void SetEntraApiUrlDomain(string value)
            => ConfigReader.Set(Identity.Constants.Environment.EntraAPIurlDomain, value);

        private static PatchRegisteredUserRequest CreatePatchRegisteredUserRequest(string memberNumber, string emailAddress)
        {
            return new PatchRegisteredUserRequest
            {
                MemberNumber = memberNumber,
                EmailAddress = emailAddress,
                FirstName = "Jane",
                LastName = "Doe",
                CustomerId = "customer-1",
                CountryCode = "GB",
                MemberType = "Individual"
            };
        }

        private static CustomerAccountDetails CreateValidRequest()
        {
            return new CustomerAccountDetails
            {
                FirstName = "Jane",
                LastName = "Doe",
                Gender = "female",
                DateOfBirth = "1990-01-01",
                Password = "Password@123",
                FcNumber = "1234567890",
                CustomerId = "customer-1",
                CustomerType = CustomerAccountTypeEnum.Loyalty,
                Status = "ACTIVE",
                ContactDetails = new AccountContactDetails
                {
                    EmailDetails = new List<AccountEmailDetail>
                    {
                        new AccountEmailDetail
                        {
                            EmailId = "JANE.DOE@EXAMPLE.COM",
                            PreferredFlag = "Y"
                        }
                    },
                    PhoneDetails = new List<AccountPhoneDetail>
                    {
                        new AccountPhoneDetail
                        {
                            CountryCode = "+44",
                            PhoneNumber = "1234567890",
                            PreferredFlag = "Y"
                        }
                    }
                },
                AddressDetails = new List<AccountAddressDetail>
                {
                    new AccountAddressDetail
                    {
                        AddressLine1 = "1 Test Street",
                        City = "London",
                        Country = "GB",
                        PostalCode = "SW1A-1AA",
                        PreferredFlag = "Y"
                    }
                },
                LoyaltyMember = new AccountLoyaltyMember
                {
                    MemberType = AccountLoyaltyMember.AccountMemberTypes.Individual,
                    EnrollmentChannel = "WEB-MYA",
                    TermsAndConditionsAccepted = true,
                    FlyingClubCardNumber = "1234567890"
                },
                Preferences = new List<AccountPreference>
                {
                    new AccountPreference
                    {
                        PreferenceType = AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions,
                        PreferenceValue = "Y"
                    }
                },
                ContinuationToken = "test-continuation-token"
            };
        }

    }
}
