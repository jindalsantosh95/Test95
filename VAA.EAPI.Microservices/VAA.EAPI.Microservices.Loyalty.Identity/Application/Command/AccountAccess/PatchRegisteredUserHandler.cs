using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Mapper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using GraphIdentityType = VAA.EAPI.Domain.Models.B2CGraph.IdentityType;
using RequestedIdentityType = VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C.IdentityType;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class PatchRegisteredUserHandler(
        IVerifyMemberStatusRepository verifyMemberStatusRepository,
        IUpdateAdUserService updateAdUserService,
        ILogger<PatchRegisteredUserHandler> logger) : IPatchRegisteredUserHandler
    {
        private const int RegisteredUserLookupDelayMilliseconds = 500;

        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
        private readonly IUpdateAdUserService _updateAdUserService = updateAdUserService ?? throw new ArgumentNullException(nameof(updateAdUserService));
        private readonly ILogger<PatchRegisteredUserHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Handle(PatchRegisteredUserRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var emailAddress = request.EmailAddress?.Trim().ToLowerInvariant();

            if (emailAddress.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Patch registered user request email address is missing.");

            request.EmailAddress = emailAddress;

            var memberInfo = await GetRegisteredUserWithRetry(emailAddress, cancellationToken);

            if ((memberInfo?.Id).IsNullOrWhiteSpace())
            {
                _logger.LogWarning("Unable to find newly registered Entra account for CompleteSignUp patch.");
                throw new InvalidOperationException("Unable to find newly registered Entra account for CompleteSignUp patch.");
            }

            var content = GetUserPatchPayload(request, memberInfo.Identities);
            var updateResponse = await _updateAdUserService.UpdateAdUser($"/users/{memberInfo.Id}", null, content, HttpMethod.Patch, cancellationToken);

            if (updateResponse?.IsB2COperationSuccess != true)
            {
                _logger.LogWarning("Unable to patch newly registered Entra account for CompleteSignUp.");
                throw new InvalidOperationException("Unable to patch newly registered Entra account for CompleteSignUp.");
            }
        }

        private async Task<MemberInfo> GetRegisteredUserWithRetry(string emailAddress, CancellationToken cancellationToken)
        {
            var retryCounter = Math.Max(1, ConfigReader.Try(Environment.RetryCounter, 1));
            MemberInfo memberInfo = null;

            for (var attempt = 1; attempt <= retryCounter; attempt++)
            {
                memberInfo = await _verifyMemberStatusRepository.VerifyMemberStatus(emailAddress, string.Empty, cancellationToken);

                if (!(memberInfo?.Id).IsNullOrWhiteSpace())
                    return memberInfo;

                if (attempt < retryCounter)
                {
                    _logger.LogWarning("Newly registered Entra account was not available for CompleteSignUp patch lookup. Retrying attempt {Attempt} of {RetryCounter}.", attempt + 1, retryCounter);
                    await Task.Delay(RegisteredUserLookupDelayMilliseconds, cancellationToken);
                }
            }

            return memberInfo;
        }

        private static string GetUserPatchPayload(PatchRegisteredUserRequest request, List<GraphIdentityType> existingIdentities)
        {
            var b2cDomain = ConfigReader.Get(Environment.B2CDomain);

            var requestedIdentities = new List<RequestedIdentityType>();

            if (!request.EmailAddress.IsNullOrWhiteSpace())
            {
                requestedIdentities.Add(new RequestedIdentityType
                {
                    SignInType = ConfigReader.Get(Environment.SignInEmailAddress),
                    Issuer = b2cDomain,
                    IssuerAssignedId = request.EmailAddress
                });
            }

            if (!request.MemberNumber.IsNullOrWhiteSpace())
            {
                requestedIdentities.Add(new RequestedIdentityType
                {
                    SignInType = ConfigReader.Get(Environment.SignInUserName),
                    Issuer = b2cDomain,
                    IssuerAssignedId = request.MemberNumber
                });
            }

            return Json.Serialize(new Dictionary<string, object>
            {
                { ConfigReader.Get(Environment.AccountEnabled), true },
                { ConfigReader.Get(Environment.Surname), request.LastName },
                { ConfigReader.Get(Environment.DisplayName), request.FirstName },
                { ConfigReader.Get(Environment.GivenName), request.FirstName },
                { ConfigReader.Get(Environment.Identities), GetUpdatedIdentities(existingIdentities, requestedIdentities) },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberTypeAttribute)}", request.MemberType },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}", Constants.Application.MemberStatus },
                { $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomCustomerIdAttribute)}", request.CustomerId },
            }, false);
        }

        private static List<GraphIdentityType> GetUpdatedIdentities(List<GraphIdentityType> existingIdentities, List<RequestedIdentityType> requestedIdentities)
        {
            var updatedIdentities = !existingIdentities.IsNullOrEmpty()
                ? existingIdentities.Select(identity => new GraphIdentityType
                {
                    SignInType = identity.SignInType,
                    Issuer = identity.Issuer,
                    IssuerAssignedId = identity.IssuerAssignedId
                }).ToList()
                : requestedIdentities?.Select(identity => new GraphIdentityType
                {
                    SignInType = identity.SignInType,
                    Issuer = identity.Issuer,
                    IssuerAssignedId = identity.IssuerAssignedId
                }).ToList() ?? new List<GraphIdentityType>();

            var requestedMemberIdentity = requestedIdentities?
                .FirstOrDefault(identity => string.Equals(identity.SignInType, ConfigReader.Get(Environment.SignInUserName), StringComparison.OrdinalIgnoreCase));

            if (requestedMemberIdentity == null || requestedMemberIdentity.IssuerAssignedId.IsNullOrWhiteSpace())
                return updatedIdentities;

            var memberIdentity = updatedIdentities
                .FirstOrDefault(identity => string.Equals(identity.SignInType, requestedMemberIdentity.SignInType, StringComparison.OrdinalIgnoreCase));

            if (memberIdentity == null)
            {
                updatedIdentities.Add(new GraphIdentityType
                {
                    SignInType = requestedMemberIdentity.SignInType,
                    Issuer = requestedMemberIdentity.Issuer,
                    IssuerAssignedId = requestedMemberIdentity.IssuerAssignedId
                });
            }
            else
            {
                memberIdentity.Issuer = memberIdentity.Issuer.IsNullOrWhiteSpace() ? requestedMemberIdentity.Issuer : memberIdentity.Issuer;
                memberIdentity.IssuerAssignedId = requestedMemberIdentity.IssuerAssignedId;
            }

            return updatedIdentities;
        }
    }
}
