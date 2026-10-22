using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class AccountLinkingEligibilityService : IAccountLinkingEligibilityService
    {
        private readonly IAccountCustomerService _customers;

        public AccountLinkingEligibilityService(IAccountCustomerService customers) =>
            _customers = customers ?? throw new ArgumentNullException(nameof(customers));

        public async Task<string> ResolveFlyingClubAccountAsync(ClaimsPrincipal principal)
        {
            var emails = principal?.Claims
                .Where(claim => claim.Type.Is(ClaimTypes.Email) || claim.Type.Is("email") || claim.Type.Is("emails"))
                .Select(claim => claim.Value)
                .Where(value => !value.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];
            if (emails.Length == 0)
            {
                emails = principal?.Claims
                    .Where(claim => claim.Type.Is("preferred_username"))
                    .Select(claim => claim.Value)
                    .Where(value => !value.IsNullOrWhiteSpace())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray() ?? [];
                if (emails.Length != 1 ||
                    !MailAddress.TryCreate(emails[0], out var address) ||
                    !address.Address.Is(emails[0]))
                    throw InvalidProfile();
            }
            if (emails.Length != 1)
                throw InvalidProfile();

            var profiles = await _customers.GetCustomerDetailsByEmail(new CustomerProfileRequest { Email = emails[0] });
            if (profiles?.Count != 1)
                throw InvalidProfile();

            var profile = profiles[0];
            if (profile is null)
                throw InvalidProfile();
            var normalizedStatus = RegexCompiled.Replace(profile.Status ?? string.Empty, "\\s", string.Empty);
            if (!normalizedStatus.Is("Active") ||
                profile.LoyaltyMemberNumber.IsNullOrWhiteSpace() ||
                !RegexCompiled.IsMatch(profile.LoyaltyMemberNumber, "^[0-9]{1,20}$", RegexOptions.CultureInvariant))
                throw InvalidProfile();

            return profile.LoyaltyMemberNumber;
        }

        private static RequestValidationException InvalidProfile() =>
            new("Exactly one active Flying Club profile is required.");
    }
}
