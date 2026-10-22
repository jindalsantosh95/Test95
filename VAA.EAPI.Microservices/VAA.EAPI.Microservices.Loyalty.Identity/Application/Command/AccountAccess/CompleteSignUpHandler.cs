using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class CompleteSignUpHandler(ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, IEntraService entraService, ILogger<CompleteSignUpHandler> logger) : ICompleteSignUpHandler
    {
        private static readonly IReadOnlyList<string> AuthenticationScopes = new List<string> { "openid", "offline_access", "profile", "email" };

        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        private readonly IEntraService _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
        private readonly ILogger<CompleteSignUpHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Response<TokenResponse>> CompleteSignUp(CustomerAccountDetails request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "Identity:CompleteSignUp"
            };
          
            try
            {
                var validationError = ValidateRequiredFields(request);
                if (validationError is not null)
                    return validationError;

                var emailId = request?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim().ToLowerInvariant();

                request.Gender = NormalizeGender(request?.Gender);

                var phoneDetails = request?.ContactDetails?.PhoneDetails;
                if (phoneDetails != null)
                {
                    foreach (var p in phoneDetails)
                    {
                        if (!string.IsNullOrWhiteSpace(p.CountryCode) && p.CountryCode.Trim().StartsWith("+"))
                            p.CountryCode = p.CountryCode.Trim().TrimStart('+');
                    }
                }

                var addressDetails = request?.AddressDetails;
                if (addressDetails != null)
                {
                    foreach (var a in addressDetails)
                    {
                        if (a.AddressType == null && a.PreferredFlag == "Y")
                            a.AddressType = AccountAddressDetail.AccountAddressTypes.Home;
                    }
                }

                PatchRegisteredUserRequest patchRegisteredUserRequest = new()
                {
                    MemberNumber = request?.FcNumber,
                    EmailAddress = emailId,
                    FirstName = request?.FirstName,
                    LastName = request?.LastName,
                    CustomerId = request?.CustomerId,
                    CountryCode = request?.AddressDetails?.FirstOrDefault()?.Country,
                    MemberType = request?.LoyaltyMember?.MemberType.ToString()
                };

                trackAnalyticsEventCommand.Track("MemberType", request?.LoyaltyMember?.MemberType);

                (bool flowControl, Response<TokenResponse> response) = await CompleteAuthRegistrationAsync(request, cancellationToken);
                if (!flowControl)
                    return response;

                var authenticationContinuationToken = response.Value?.ContinuationToken;

                var correlationIdForEvents = Guid.NewGuid().ToString();
                var patchRegisteredUserCloudEvent = PatchRegisteredUserEventMapper.EventDataMapper(correlationIdForEvents, patchRegisteredUserRequest);
                var emailEventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaDatawithWelcomeEmail(MapCustomerDetailsToCustomerEntities.MaptoCreateCustomerEntities(request));
                var emailCloudEvent = EmailTriggeringCloudEventMapper.EventDataMapper(emailEventData);

                if (request?.AddressDetails != null && request.AddressDetails.Any(x => !string.IsNullOrWhiteSpace(x.PostalCode) && x.PostalCode.Contains('-')))
                {
                    foreach (var item in request.AddressDetails)
                    {
                        if (!string.IsNullOrWhiteSpace(item.PostalCode))
                            item.PostalCode = item.PostalCode.Replace("-", "");
                    }
                }
                var customerSyncCloudEvent = CustomerSyncEventMapper.EventDataMapper(correlationIdForEvents, request);
                var vredOcdbSyncCloudEvent = VredOcdbSyncEventMapper.EventDataMapper(correlationIdForEvents, request);
                var vredMarketingPreferenceCloudEvent = VredMarketingPreferenceEventMapper.EventDataMapper(correlationIdForEvents, request);
                var vredPreferenceData = vredMarketingPreferenceCloudEvent?.Data as ConsentReceiptRequest;

                var tasks = new List<Task>
                {
                     PublishToPatchRegisteredUserEvent(patchRegisteredUserCloudEvent, correlationIdForEvents, cancellationToken),
                     PublishToEnrolCustomerEvent(customerSyncCloudEvent, correlationIdForEvents, cancellationToken),
                     PublishToVredOcDbSync(vredOcdbSyncCloudEvent, correlationIdForEvents, cancellationToken),
                     PublishToEmailEvent(emailCloudEvent, correlationIdForEvents, cancellationToken)
                };

                if (!string.IsNullOrEmpty(vredPreferenceData?.MarketingPreference) && ConfigReader.Get(Constants.Application.VredMarketingFeatureFlag) == "Y")
                    tasks.Add(PublishToMarketingPreferenceEvent(vredMarketingPreferenceCloudEvent, correlationIdForEvents, cancellationToken));

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while publishing cloud events");
                }

                return await CompleteAuthentication(authenticationContinuationToken, emailId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete SignUp for member number failed");
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during Completion of SignUp");
            }
            finally
            {
                try
                {
                    await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish analytics event for {Subject}", trackAnalyticsEventCommand.Subject);
                }
            }
        }

        private async Task<(bool flowControl, Response<TokenResponse> value)> CompleteAuthRegistrationAsync(CustomerAccountDetails request, CancellationToken cancellationToken)
        {
            var challengeTypes = new List<ChallengeType> { ChallengeType.Oob, ChallengeType.Password, ChallengeType.Redirect };

            var challengeResponse = await _entraService.Challenge(challengeTypes, "signup/v1.0", request.ContinuationToken, cancellationToken);
            var challengeJson = JsonConvert.DeserializeObject<JObject>(challengeResponse);

            if (challengeJson == null || challengeJson["error"] != null)
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateErrorResponse(challengeJson));

            if (challengeJson["challenge_type"]?.ToString() == "redirect")
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateRedirectResponse());

            var continuationToken = challengeJson["continuation_token"]?.ToString();
            var challengeType = challengeJson["challenge_type"]?.ToString();

            if (string.IsNullOrEmpty(continuationToken))
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication"));

            if (string.IsNullOrEmpty(challengeType))
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication"));

            var tokenResponse = await _entraService.Continue(continuationToken, "password", request.Password, "signup", cancellationToken);
            
            var tokenJson = JsonConvert.DeserializeObject<JObject>(tokenResponse);

            if (tokenJson == null || (tokenJson["error"] != null && tokenJson["continuation_token"] == null))
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson));

            continuationToken = tokenJson["continuation_token"]?.ToString();

            if (string.IsNullOrEmpty(continuationToken))
                return (flowControl: false, value: EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication"));

            return (flowControl: true, value: new Response<TokenResponse>()
            {
                Status = Status.OK,
                Value = new TokenResponse
                {
                    ContinuationToken = continuationToken
                },
                Errors = null
            });
        }

        private async Task<Response<TokenResponse>> CompleteAuthentication(string continuationToken, string emailAddress, CancellationToken cancellationToken, bool allowMfaRegistration = true)
        {
            string tokenResponse = await _entraService.Token(continuationToken, new List<string> { "continuation_token" }, AuthenticationScopes, null, null, null, cancellationToken);

            var tokenJson = JsonConvert.DeserializeObject<JObject>(tokenResponse);

            if (tokenJson == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson);

            // A complete token set is terminal. A continuation token alongside it must not
            // start a second native-registration journey.
            if (HasTokenPayload(tokenJson))
                return CreateTokenResponse(tokenJson);

            var registrationContinuationToken = tokenJson["continuation_token"]?.ToString();

            if (allowMfaRegistration && IsMfaRegistrationRequired(tokenJson))
            {
                if (registrationContinuationToken.IsNullOrWhiteSpace())
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid registration response: missing continuation token");

                return await CompleteMfaRegistration(registrationContinuationToken, emailAddress, cancellationToken);
            }

            if (tokenJson["error"] != null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson);

            return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid authentication response: no token set returned");
        }

        private async Task<Response<TokenResponse>> CompleteMfaRegistration(string continuationToken, string emailAddress, CancellationToken cancellationToken)
        {
            var introspectResponse = await _entraService.RegisterIntrospect(continuationToken, cancellationToken);
            var introspectJson = JsonConvert.DeserializeObject<JObject>(introspectResponse);

            if (introspectJson == null || introspectJson["error"] != null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(introspectJson);

            var registrationContinuationToken = introspectJson["continuation_token"]?.ToString();
            var emailMethod = (introspectJson["methods"] as JArray)?
                .OfType<JObject>()
                .FirstOrDefault(method =>
                    (method["challenge_type"]?.ToString()).Is("oob") &&
                    (method["challenge_channel"]?.ToString()).Is("email"));

            if (registrationContinuationToken.IsNullOrWhiteSpace() || emailMethod == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "registration_method_unavailable", "Email MFA registration is not available for this account");

            var challengeTarget = emailMethod["challenge_target"]?.ToString() ?? emailAddress;
            if (challengeTarget.IsNullOrWhiteSpace())
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "registration_method_unavailable", "Entra did not provide an email target for MFA registration");

            var challengeResponse = await _entraService.RegisterChallenge(registrationContinuationToken, challengeTarget, cancellationToken);
            var challengeJson = JsonConvert.DeserializeObject<JObject>(challengeResponse);

            if (challengeJson == null || challengeJson["error"] != null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(challengeJson);

            var challengeType = challengeJson["challenge_type"]?.ToString();
            var challengeContinuationToken = challengeJson["continuation_token"]?.ToString();

            if (challengeType.IsNullOrWhiteSpace() || challengeContinuationToken.IsNullOrWhiteSpace())
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid registration challenge response");

            if (!challengeType.Is("preverified"))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "registration_not_preverified", "Entra MFA registration was not preverified for the enrolment email");

            var continueResponse = await _entraService.RegisterContinue(challengeContinuationToken, "continuation_token", cancellationToken: cancellationToken);
            var continueJson = JsonConvert.DeserializeObject<JObject>(continueResponse);

            if (continueJson == null || continueJson["error"] != null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(continueJson);

            var finalContinuationToken = continueJson["continuation_token"]?.ToString();
            if (finalContinuationToken.IsNullOrWhiteSpace())
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid registration continuation response");

            return await CompleteAuthentication(finalContinuationToken, emailAddress, cancellationToken, allowMfaRegistration: false);
        }

        private static Response<TokenResponse> CreateTokenResponse(JObject tokenJson)
        {
            return new Response<TokenResponse>
            {
                Status = Status.OK,
                Value = new TokenResponse
                {
                    ContinuationToken = tokenJson["continuation_token"]?.ToString(),
                    AccessToken = tokenJson["access_token"]?.ToString(),
                    RefreshToken = tokenJson["refresh_token"]?.ToString(),
                    IdToken = tokenJson["id_token"]?.ToString(),
                    ExpiresIn = tokenJson["expires_in"]?.ToObject<int>() ?? 0,
                    Scope = string.Join(" ", AuthenticationScopes),
                    TokenType = tokenJson["token_type"]?.ToString()
                }
            };
        }

        private static bool HasTokenPayload(JObject tokenJson)
        {
            return !(tokenJson["access_token"]?.ToString()).IsNullOrWhiteSpace() &&
                   !(tokenJson["refresh_token"]?.ToString()).IsNullOrWhiteSpace() &&
                   !(tokenJson["id_token"]?.ToString()).IsNullOrWhiteSpace();
        }

        private static bool IsMfaRegistrationRequired(JObject tokenJson)
        {
            return (tokenJson["suberror"]?.ToString()).Is("registration_required");
        }

        private static string? NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return null;

            return gender.ToLowerInvariant() switch
            {
                "female" => "F",
                "male" => "M",
                "undisclosed" => "U",
                "unspecified" => "X",
                _ => gender
            };
        }
        private async Task PublishToPatchRegisteredUserEvent(CloudEvent patchRegisteredUserCloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.PatchRegisteredUserEvent, Data = patchRegisteredUserCloudEvent.Data, Subject = patchRegisteredUserCloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = patchRegisteredUserCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
            }
            catch (Exception)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.PatchRegisteredUserEvent, Data = patchRegisteredUserCloudEvent.Data, Subject = patchRegisteredUserCloudEvent.Subject }, cancellationToken);

                throw;
            }
        }
        private async Task PublishToVredOcDbSync(CloudEvent vredOcdbSyncCloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.VredOcdbSycnEvent, Data = vredOcdbSyncCloudEvent.Data, Subject = vredOcdbSyncCloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = vredOcdbSyncCloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
            }
            catch (Exception)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.VredOcdbSycnEvent, Data = vredOcdbSyncCloudEvent.Data, Subject = vredOcdbSyncCloudEvent.Subject }, cancellationToken);

                throw;
            }
        }
        private async Task PublishToEmailEvent(CloudEvent emailEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            int retryCounter = ConfigReader.Get<int>(Constants.Environment.RetryCounter);

            for (int i = 0; i < retryCounter; i++)
            {
                try
                {
                    await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);

                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = emailEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
                    break;
                }
                catch (Exception)
                {
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);

                    if (i == retryCounter - 1)
                        throw;
                }
            }
        }
        private async Task PublishToEnrolCustomerEvent(CloudEvent enrolCustomerEventData, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.EnrolCustomerEvent, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = enrolCustomerEventData, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
            }
            catch (Exception)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.EnrolCustomerEvent, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task PublishToMarketingPreferenceEvent(CloudEvent enrolCustomerEventData, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = enrolCustomerEventData.Type, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = enrolCustomerEventData, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
            }
            catch (Exception)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = enrolCustomerEventData.Type, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);

                throw;
            }
        }
        private Response<TokenResponse>? ValidateRequiredFields(CustomerAccountDetails request)
        {
            if (request == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.Gender))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Gender is required");

            if (string.IsNullOrWhiteSpace(request.DateOfBirth))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Date of Birth is required");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "First Name is required");

            if (string.IsNullOrWhiteSpace(request.LastName))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Last Name is required");

            if (string.IsNullOrWhiteSpace(request.CustomerId))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Customer Id is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Password is required");

            if (string.IsNullOrWhiteSpace(request.ContinuationToken))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Continuation token is required");

            if (string.IsNullOrWhiteSpace(request.FcNumber))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Member number is required");

            if (string.IsNullOrWhiteSpace(request.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Email address is required");

            if (request.LoyaltyMember == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Loyalty member details are required");

            if (string.IsNullOrWhiteSpace(request.LoyaltyMember.EnrollmentChannel))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Enrolment channel is required");

            if (request.AddressDetails == null || !request.AddressDetails.Any())
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "At least one address detail is required");

            if (request.AddressDetails.Any(x => x.Country == null))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Address country code is required");

            return null;
        }
    }
}
