using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Http;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class VerifyMemberStatusRepository : IVerifyMemberStatusRepository
    {
        private readonly IGetAdUserDetailsService _getUserDetailsService;
        private readonly ILogger<VerifyMemberStatusRepository> _logger;


        public VerifyMemberStatusRepository(IGetAdUserDetailsService getUserDetailsService, ILogger<VerifyMemberStatusRepository> logger)
        {
            _getUserDetailsService = getUserDetailsService ?? throw new ArgumentNullException( nameof(getUserDetailsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MemberInfo> VerifyMemberStatus(string username, string oid, CancellationToken cancellationToken)
        {
            var graphResponse = username.IsNullOrWhiteSpace() ? await _getUserDetailsService.GetDetailsFromGraphApi($"/users/{oid}", GetB2CQueryWithId(), cancellationToken) : await _getUserDetailsService.GetDetailsFromGraphApi("/users", GetB2CQuery(username), cancellationToken);

            if (graphResponse.IsNullOrWhiteSpace())
                return null;

            MemberInfo memberInfo = new();
            JsonDocument doc = JsonDocument.Parse(graphResponse);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("value", out JsonElement users) && users.GetArrayLength() > 0)
            {
                JsonElement firstItem = users[0];
                string expectedClaim = $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}";

                foreach (JsonProperty property in firstItem.EnumerateObject())
                {
                    if (property.Name.Is(expectedClaim))
                    {
                        memberInfo.MemberStatus = property.Value.GetString();
                        break;
                    }
                }

                B2CGraphResponse customerResponse = Json.Deserialize<B2CGraphResponse>(graphResponse);
                var customer = customerResponse?.Value?.FirstOrDefault();

                if (customer != null)
                {
                    var identities = customer.Identities;
                    memberInfo.MemberNumber = identities?.FirstOrDefault(identity => identity.SignInType.Is("username"))?.IssuerAssignedId;
                    memberInfo.MemberEmail = identities?.FirstOrDefault(identity => identity.SignInType.Is("emailAddress"))?.IssuerAssignedId;
                    memberInfo.Identities = identities;
                    memberInfo.Id = customer.Id;
                }
            }
            else
            {
                string expectedClaim = $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}";

                foreach (JsonProperty property in root.EnumerateObject())
                {
                    if (property.Name.Is(expectedClaim))
                    {
                        memberInfo.MemberStatus = property.Value.GetString();
                        break;
                    }
                }

                var response = Json.Deserialize<Value>(graphResponse);
                memberInfo.Id = response?.Id;
                var identities = response?.Identities;
                memberInfo.MemberNumber = identities?.FirstOrDefault(identity => identity.SignInType.Is("username"))?.IssuerAssignedId;
                memberInfo.MemberEmail = identities?.FirstOrDefault(identity => identity.SignInType.Is("emailAddress"))?.IssuerAssignedId;
                memberInfo.Identities = identities;
            }

            return memberInfo;
        }

        public async Task<(bool IsValid, string? EmailAuthMethodId, string? MfaEmail)> VerifyMemberAuthStatus(string oid, string email, CancellationToken cancellationToken)
        {
            string graphResponse;
            try
            {
                graphResponse = await _getUserDetailsService
                    .GetDetailsFromGraphApi($"/users/{oid}/authentication/emailMethods", string.Empty, cancellationToken);

                if (graphResponse.IsNullOrWhiteSpace()) return (false, null, null);

                using var doc = JsonDocument.Parse(graphResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("value", out var methods) && methods.ValueKind == JsonValueKind.Array)
                {
                    foreach (var method in methods.EnumerateArray())
                    {
                        if (method.TryGetProperty("emailAddress", out var emailProp))
                        {
                            var mfaEmail = emailProp.GetString();
                            method.TryGetProperty("id", out var emailAuthMethodId);
                            if (!mfaEmail.IsNullOrWhiteSpace())
                                return (true, emailAuthMethodId.GetString(), mfaEmail);
                        }
                    }
                }
                return (false, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(VerifyMemberStatusRepository)}-Exception fecthing auth methods : {ex.Message}");
                return (false, null, null);
            }
        }


        private static string GetB2CQuery(string emailID)
        {
            if (!string.IsNullOrEmpty(emailID))
            {
                var memberStatus = $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}";
                return $"$select=Id,Identities,{memberStatus}" +
                $"&$filter={ConfigReader.Get(Environment.Identities)}/any(x: x / issuer eq {"'"}{ConfigReader.Get(Environment.B2CDomain)}{"'"} and issuerAssignedId eq {"'"}{HttpUtility.UrlEncode(emailID)}{"'"})";

            }
            return string.Empty;
        }

        private static string GetB2CQueryWithId()
        {
            var memberStatus = $"{ConfigReader.Get(Environment.ExtensionPrefix)}{ConfigReader.Get(Environment.CustomMemberStatusAttribute)}";
            return $"$select=Id,Identities,{memberStatus}";
        }
    }
}
