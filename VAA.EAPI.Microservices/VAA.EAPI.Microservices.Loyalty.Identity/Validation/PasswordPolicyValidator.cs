using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Validation
{
    public class PasswordPolicyValidator
    {
        public static Task<IEnumerable<PasswordPolicy>> Verify(string password)
        {
            List<PasswordPolicyRule> policies = new()
            {
                new() {PolicyId = "PP01", PolicyDescription = "Length must be between 8 and 20 characters", IsValid = false, Pattern = "[^a-zA-Z]|[a-zA-Z]", MinOccurances = 8, MaxOccurances = 20},
                new() {PolicyId = "PP02", PolicyDescription = "Must contain at least 1 number", IsValid = false, Pattern = "[0-9]", MinOccurances = 1, MaxOccurances = 999},
                new() {PolicyId = "PP03", PolicyDescription = "Must contain at least 1 upper case letter", IsValid = false, Pattern = "[A-Z]", MinOccurances = 1, MaxOccurances = 999},
                new() {PolicyId = "PP04", PolicyDescription = "Must contain at least 1 lower case letter", IsValid = false, Pattern = "[a-z]", MinOccurances = 1, MaxOccurances = 999},
                new() {PolicyId = "PP05", PolicyDescription = "Must contain no more than 3 special characters", IsValid = false, Pattern = "[^a-zA-Z0-9 ]", MinOccurances = 0, MaxOccurances = 3},
                new() {PolicyId = "PP06", PolicyDescription = "Must not contain the '@' symbol", IsValid = false, Pattern = "@", MinOccurances = 0, MaxOccurances = 0},
            };

            policies.ForEach(p =>
            {
                var matches = Regex.Matches(password, p.Pattern, RegexOptions.Singleline).Count;
                p.IsValid = !(string.IsNullOrEmpty(password)) && matches >= p.MinOccurances && matches <= p.MaxOccurances;
            });

            return Task.FromResult(policies.Select(p => new PasswordPolicy()
            {
                PolicyId = p.PolicyId,
                PolicyDescription = p.PolicyDescription,
                IsValid = p.IsValid
            }));
        }
    }
}
