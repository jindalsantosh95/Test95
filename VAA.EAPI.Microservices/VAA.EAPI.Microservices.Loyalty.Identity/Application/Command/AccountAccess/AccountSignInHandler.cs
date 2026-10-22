using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Diagnostics.Telemetry;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class AccountSignInHandler : IAccountSignInHandler
    {
        private const string EntraDiagnosticsEventName = "EntraDiags";

        private readonly ICustomEventTracker _customEventTracker;
        private readonly IEntraService _entraService;
        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository;
        private readonly IUpdateAdUserService _updateAdUserService;
        private readonly IMediator _mediator;
        private readonly bool _entraLogging = ConfigReader.Get<bool>(Environment.EntraLogging);


        public AccountSignInHandler(ICustomEventTracker customEventTracker, IEntraService entraService, IVerifyMemberStatusRepository verifyMemberStatusRepository, IUpdateAdUserService updateADUserService, IMediator mediator)
        {
            _customEventTracker = customEventTracker ?? throw new ArgumentNullException(nameof(customEventTracker));
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
            _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
            _updateAdUserService = updateADUserService ?? throw new ArgumentNullException(nameof(updateADUserService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Response<TokenResponse>> AccountSignIn(AccountSignInRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                if (_entraLogging)
                    TrackEntraDiagnosticsEvent("Missing continuation token", "Information");
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                if (_entraLogging)
                    TrackEntraDiagnosticsEvent("Missing username", "Information", request);
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Username is required");
            }

            if (_entraLogging)
                TrackEntraDiagnosticsEvent("Sign in started", "Information", request);

            try
            {
                //Initiate
                var challengeTypes = DetermineChallengeTypes(request);

                Domain.MemberInfo customerResponse = null;

                if (request.FlyingClubNumber.IsNullOrEmpty())
                    customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(request.Email, string.Empty, cancellationToken);
                else
                     customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(request.FlyingClubNumber, string.Empty, cancellationToken);
                
                if(customerResponse == null)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Error occurred during status check: Email/Flying Club Number not found associated to account in Azure", "Error", request);
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.B2CBusinessError, "Account is not Azure", "Account did not meet the required status to log in");
                }

                if (!string.Equals(customerResponse.MemberEmail, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    TrackEntraDiagnosticsEvent("Email mismatch for Flying Club account. Patching.", "Warning", request, new Dictionary<string, string>
                    {
                        ["CustomerId"] = customerResponse.Id,
                        ["MemberNumber"] = customerResponse.MemberNumber,
                        ["OldEmail"] = customerResponse.MemberEmail
                    });

                    if (request.CustomerProfile != null && !string.IsNullOrWhiteSpace(customerResponse.MemberEmail) && !string.IsNullOrWhiteSpace(request.CustomerProfile.Email))
                    {
                        try
                        {
                            var patchSucceeded = await PatchEmailForUnsyncedAccountMethodForUser(request.CustomerProfile.Email, customerResponse.Id, customerResponse.Identities, cancellationToken);
                            if (patchSucceeded)
                            {
                                TrackB2CEmailSyncRepairEvent(customerResponse, request, "PatchAndPublish");
                                await PublishEmailSyncEvents(request.CustomerProfile, cancellationToken);
                            }
                            else
                            {
                                TrackEntraDiagnosticsEvent("Skipping email sync event publication because B2C patch repair did not succeed.", "Warning", request);
                            }
                        }
                        catch (Exception ex)
                        {
                            TrackEntraDiagnosticsEvent("Failed to repair unsynced account email.", "Error", request, new Dictionary<string, string>
                            {
                                ["CustomerId"] = customerResponse.Id,
                                ["MemberNumber"] = customerResponse.MemberNumber,
                                ["OldEmail"] = customerResponse.MemberEmail,
                                ["NewEmail"] = request.CustomerProfile.Email
                            }, ex);
                        }
                    }
                }

                var initiateResponse = await _entraService.Initiate(challengeTypes, request.Email, cancellationToken);
                var initiateJson = JsonConvert.DeserializeObject<JObject>(initiateResponse);

                if (initiateJson == null || initiateJson["error"] != null)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Error occurred during Entra initiate", "Error", request, new Dictionary<string, string>
                        {
                            ["InitiateResponse"] = initiateJson?.ToString()
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(initiateJson);
                }

                if (initiateJson["challenge_type"]?.ToString() == "redirect")
                    return EntraErrorHelper<TokenResponse>.CreateRedirectResponse();

                var continuationToken = initiateJson["continuation_token"]?.ToString();

                if (string.IsNullOrEmpty(continuationToken))
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Entra sign in missing continuation token from initiate", "Information", request);
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid authentication response: missing continuation token");
                }

                bool isActive = customerResponse.MemberStatus != null && customerResponse.MemberStatus
                    .Replace(" ", string.Empty)
                    .IsAny("active", "inactive", "activepending");

                if (!isActive)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("VerifyMemberStatus returned invalid", "Information", request, new Dictionary<string, string>
                        {
                            ["MemberNumber"] = customerResponse.MemberNumber,
                            ["MemberStatus"] = customerResponse.MemberStatus
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.B2CBusinessError, "Account is not Active", "Account did not meet the required status to log in");
                }

                await DisablePasswordPoliciesForUser(customerResponse.Id, cancellationToken);

                var (authMethodEnabled, emailAuthMethodId, mfaEmail) = await _verifyMemberStatusRepository.VerifyMemberAuthStatus(customerResponse.Id, request.Email, cancellationToken);

                if (!authMethodEnabled)
                {
                    var created = await CreateEmailAuthenticationMethodForUser(request.Email, customerResponse.Id, cancellationToken);

                    if (!created)
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.B2CBusinessError, "OTP fail status", "MFA email method could not be created, user will not receive OTP");
                }
                else if (!string.Equals(mfaEmail, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var updated = await UpdateEmailAuthenticationMethodForUser(request.Email, customerResponse.Id, emailAuthMethodId, cancellationToken);

                    if (!updated)
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.B2CBusinessError, "OTP fail status", "MFA email does not match and failed to update, user will not receive OTP");
                }

                // Challenge
                var challengeResponse = await _entraService.Challenge(challengeTypes, "oauth2/v2.0", continuationToken, cancellationToken);
                var challengeJson = JsonConvert.DeserializeObject<JObject>(challengeResponse);

                if (challengeJson == null || challengeJson["error"] != null)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Challenge failed", "Information", request, new Dictionary<string, string>
                        {
                            ["ChallengeResponse"] = challengeJson?.ToString()
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(challengeJson);
                }

                if (challengeJson["challenge_type"]?.ToString() == "redirect")
                    return EntraErrorHelper<TokenResponse>.CreateRedirectResponse();

                continuationToken = challengeJson["continuation_token"]?.ToString();
                var challengeType = challengeJson["challenge_type"]?.ToString();

                if (string.IsNullOrEmpty(continuationToken))
                {
                    TrackEntraDiagnosticsEvent("Invalid authentication response: missing continuation token after challenge", "Error", request);
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication");
                }

                if (string.IsNullOrEmpty(challengeType))
                {
                    TrackEntraDiagnosticsEvent("Invalid authentication response: missing challenge type", "Error", request);
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication");
                }

                //Token
                return await CompleteAuthentication(request, challengeType, continuationToken, cancellationToken);
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Unexpected error during sign-in process", "Error", request, exception: ex);
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during authentication");
            }
        }

        private List<ChallengeType> DetermineChallengeTypes(AccountSignInRequest request)
        {
            if (request.Password == null && request.OTP != null)
                return new List<ChallengeType> { ChallengeType.Oob, ChallengeType.Redirect };

            return new List<ChallengeType> { ChallengeType.Password, ChallengeType.Oob, ChallengeType.Redirect };
        }

        private async Task<Response<TokenResponse>> CompleteAuthentication(AccountSignInRequest request, string challengeType, string continuationToken, CancellationToken cancellationToken)
        {
            var scopes = new List<string> { "openid", "offline_access", "profile", "email", ConfigReader.Get(Environment.EntraAPIScope) };
            string tokenResponse;

            switch (challengeType)
            {
                case "password":
                    if (string.IsNullOrEmpty(request.Password))
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "missing_password", "Password is required");

                    tokenResponse = await _entraService.Token(continuationToken, new List<string> { "password" }, scopes, request.Email, request.Password, null, cancellationToken);
                    break;

                case "oob":
                    if (string.IsNullOrEmpty(request.OTP))
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "missing_otp", "One-time passcode is required");

                    tokenResponse = await _entraService.Token(continuationToken, new List<string> { "oob" }, scopes, request.Email, null, request.OTP, cancellationToken);
                    break;

                default:
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Unsupported authentication method", "Information", request, new Dictionary<string, string>
                        {
                            ["ChallengeType"] = challengeType
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unsupported_method", $"Unsupported authentication method: {challengeType}");

            }

            var tokenJson = JsonConvert.DeserializeObject<JObject>(tokenResponse);
            if (tokenJson == null || (tokenJson["error"] != null && tokenJson["continuation_token"] == null))
            {
                TrackEntraDiagnosticsEvent("Entra token request failed", "Information", request, new Dictionary<string, string>
                {
                    ["TokenResponse"] = tokenJson?.ToString()
                });
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson);
            }

            var continuation_token = tokenJson["continuation_token"]?.ToString();

            //MFA Challenge
            if (continuation_token != null)
            {
                var mfaIntrospectResponse = await _entraService.Introspect("oauth2/v2.0", continuation_token, cancellationToken);

                var mfaIntrospectJson = JsonConvert.DeserializeObject<JObject>(mfaIntrospectResponse);

                if (mfaIntrospectResponse == null || mfaIntrospectJson["error"] != null)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("Introspect request failed", "Information", request, new Dictionary<string, string>
                        {
                            ["IntrospectResponse"] = mfaIntrospectJson?.ToString()
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(mfaIntrospectJson);
                }

                continuationToken = mfaIntrospectJson["continuation_token"]?.ToString();

                var mfaMethods = mfaIntrospectJson["methods"] as JArray;

                var mfa_id = mfaMethods?
                    .FirstOrDefault(m => m["challenge_channel"]?.ToString() == "email")?["id"]?
                    .ToString();

                var mfaChallengeResponse = await _entraService.Challenge(new List<ChallengeType> { ChallengeType.Oob, ChallengeType.Redirect }, "oauth2/v2.0", continuation_token, cancellationToken, mfa_id);
                var mfaChallengeJson = JsonConvert.DeserializeObject<JObject>(mfaChallengeResponse);

                if (mfaChallengeResponse == null || mfaChallengeJson["error"] != null)
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("MFA challenge request failed", "Information", request, new Dictionary<string, string>
                        {
                            ["MfaChallengeResponse"] = mfaChallengeJson?.ToString()
                        });
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(mfaChallengeJson);
                }

                if (mfaChallengeJson["challenge_type"]?.ToString() == "redirect")
                {
                    if (_entraLogging)
                        TrackEntraDiagnosticsEvent("MFA challenge request redirected", "Information", request, new Dictionary<string, string>
                        {
                            ["MfaChallengeResponse"] = mfaChallengeJson?.ToString()
                        });
                    return EntraErrorHelper<TokenResponse>.CreateRedirectResponse();
                }

                continuationToken = mfaChallengeJson["continuation_token"]?.ToString();

                if (_entraLogging)
                    TrackEntraDiagnosticsEvent("Sign in successful with MFA", "Information", request);

                return new Response<TokenResponse> { Status = Status.OK, Value = new TokenResponse { ContinuationToken = continuationToken, RequiresMFA = true } };
            }

            if (_entraLogging)
                TrackEntraDiagnosticsEvent("Sign in successful without MFA", "Information", request);
            
            return new Response<TokenResponse>
            {
                Status = Status.OK,
                Value = new TokenResponse
                {
                    AccessToken = tokenJson["access_token"]?.ToString(),
                    RefreshToken = tokenJson["refresh_token"]?.ToString(),
                    IdToken = tokenJson["id_token"]?.ToString(),
                    ExpiresIn = tokenJson["expires_in"]?.ToObject<int>() ?? 0,
                    Scope = "openid offline_access email profile",
                    TokenType = tokenJson["token_type"]?.ToString()
                }
            };
        }

        private async Task<bool> CreateEmailAuthenticationMethodForUser(string email, string oid, CancellationToken cancellationToken)
        {
            try
            {
                var api = $"/users/{oid}/authentication/emailMethods";

                var payload = new { emailAddress = email };
                var content = Json.Serialize(payload);

                await _updateAdUserService.UpdateAdUser(api, null, content, HttpMethod.Post, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Error in CreateEmailAuthenticationMethodForUser", "Error", properties: new Dictionary<string, string>
                {
                    ["Email"] = email
                }, exception: ex);
                return false;
            }
        }

        private async Task<bool> PatchEmailForUnsyncedAccountMethodForUser(string email, string oid, List<IdentityType> identities, CancellationToken cancellationToken)
        {
            try
            {
                var api = $"/users/{oid}";

                var updatedIdentities = identities?.Select(identity => new IdentityType
                {
                    SignInType = identity.SignInType,
                    Issuer = identity.Issuer,
                    IssuerAssignedId = identity.IssuerAssignedId
                }).ToList() ?? new List<IdentityType>();

                var emailIdentity = updatedIdentities.FirstOrDefault(identity => identity.SignInType.Is("emailAddress"));
                if (emailIdentity != null)
                {
                    emailIdentity.IssuerAssignedId = email;
                }
                else
                {
                    updatedIdentities.Add(new IdentityType
                    {
                        SignInType = "emailAddress",
                        Issuer = ConfigReader.Get(Environment.B2CDomain),
                        IssuerAssignedId = email
                    });
                }

                var payload = new
                {
                    identities = updatedIdentities.Select(identity => new
                    {
                        signInType = identity.SignInType,
                        issuer = identity.Issuer,
                        issuerAssignedId = identity.IssuerAssignedId
                    })
                };
                var content = Json.Serialize(payload);

                await _updateAdUserService.UpdateAdUser(api, null, content, HttpMethod.Patch, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Error in PatchEmailForUnsyncedAccountMethodForUser", "Error", properties: new Dictionary<string, string>
                {
                    ["Email"] = email
                }, exception: ex);
                return false;
            }
        }

        private async Task PublishEmailSyncEvents(AccountSignInCustomerProfile customerProfile, CancellationToken cancellationToken)
        {
            if (customerProfile == null)
            {
                TrackEntraDiagnosticsEvent("Customer profile missing. Skipping OCDB/VRED email sync events.", "Warning");
                return;
            }

            var correlationIdForEvents = Guid.NewGuid().ToString();
            try
            {
                await Task.WhenAll(
                    PublishEvent(AccountSignInEventDataMapper.EventDataMapperOCDB(customerProfile), correlationIdForEvents, Constants.Application.UpdateEmailOCDBEventType, cancellationToken),
                    PublishEvent(AccountSignInEventDataMapper.EventDataMapperVRED(customerProfile), correlationIdForEvents, Constants.Application.UpdateEmailVREDEventType, cancellationToken));
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Error occurred while publishing OCDB/VRED email sync events", "Error", properties: new Dictionary<string, string>
                {
                    ["MemberNumber"] = customerProfile.MemberNumber
                }, exception: ex);
            }
        }

        private async Task PublishEvent(CloudEvent eventData, string correlationIdForEvents, string type, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = type, Data = eventData.Data, Subject = eventData.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(eventData, cancellationToken);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = type, Data = eventData.Data, Subject = eventData.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task<bool> UpdateEmailAuthenticationMethodForUser(string email, string oid, string emailAuthMethodId, CancellationToken cancellationToken)
        {
            try
            {
                var api = $"/users/{oid}/authentication/emailMethods/{emailAuthMethodId}";

                var payload = new { emailAddress = email };
                var content = Json.Serialize(payload);

                var updated = await _updateAdUserService
                    .UpdateMFADetailsFromGraphApi(api, content, cancellationToken);

                if (!updated)
                {
                    TrackEntraDiagnosticsEvent("B2C MFA method update failed", "Error", properties: new Dictionary<string, string>
                    {
                        ["CustomerId"] = oid
                    });
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Error in UpdateEmailAuthenticationMethodForUser", "Error", properties: new Dictionary<string, string>
                {
                    ["Email"] = email
                }, exception: ex);
                return false;
            }
        }

        private async Task DisablePasswordPoliciesForUser(string oid, CancellationToken cancellationToken)
        {
            try
            {
                var api = $"/users/{oid}";

                var payload = new { passwordPolicies = "DisablePasswordExpiration" };
                var content = Json.Serialize(payload);

                await _updateAdUserService.UpdateAdUser(api, null, content, HttpMethod.Patch, cancellationToken);
            }
            catch (Exception ex)
            {
                TrackEntraDiagnosticsEvent("Error in UpdateAduser password policy", "Error", properties: new Dictionary<string, string>
                {
                    ["CustomerId"] = oid
                }, exception: ex);
            }
        }

        private void TrackB2CEmailSyncRepairEvent(Domain.MemberInfo customerResponse, AccountSignInRequest request, string operation)
        {
            var properties = new Dictionary<string, string>
            {
                ["Handler"] = nameof(AccountSignInHandler),
                ["Operation"] = operation
            };

            AddPropertyIfNotEmpty(properties, "MemberNumber", customerResponse.MemberNumber);
            AddPropertyIfNotEmpty(properties, "CustomerId", customerResponse.Id);
            AddPropertyIfNotEmpty(properties, "OldEmail", customerResponse.MemberEmail);
            AddPropertyIfNotEmpty(properties, "NewEmail", request.CustomerProfile.Email);

            TrackEvent("B2CEmailSyncRepair", properties);
        }

        private void TrackEntraDiagnosticsEvent(string message, string severity, AccountSignInRequest request = null, Dictionary<string, string> properties = null, Exception exception = null)
        {
            var eventProperties = new Dictionary<string, string>
            {
                ["Handler"] = nameof(AccountSignInHandler),
                ["Message"] = message,
                ["Severity"] = severity
            };

            if (request != null)
            {
                AddPropertyIfNotEmpty(eventProperties, "Email", request.Email);
                AddPropertyIfNotEmpty(eventProperties, "FlyingClubNumber", request.FlyingClubNumber);
            }

            if (properties != null)
            {
                foreach (var property in properties)
                    AddPropertyIfNotEmpty(eventProperties, property.Key, property.Value);
            }

            if (exception != null)
            {
                AddPropertyIfNotEmpty(eventProperties, "ExceptionType", exception.GetType().FullName);
                AddPropertyIfNotEmpty(eventProperties, "ExceptionMessage", exception.Message);
            }

            TrackEvent(EntraDiagnosticsEventName, eventProperties);
        }

        private void TrackEvent(string eventName, Dictionary<string, string> properties)
        {
            _customEventTracker.TrackEvent(eventName, properties);
        }

        private static void AddPropertyIfNotEmpty(Dictionary<string, string> properties, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                properties[key] = value;
        }
    }
}
