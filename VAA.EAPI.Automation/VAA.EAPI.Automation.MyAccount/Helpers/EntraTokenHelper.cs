using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using GraphQlEndpoint = VAA.EAPI.Automation.MyAccount.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.MyAccount.Helpers
{
    /// <summary>
    /// HTTP-based bearer-token acquisition for Flying Club members via the Entra
    /// auth path — call the <c>accountMemberSignIn</c> GraphQL mutation and return
    /// the resulting JWT. Preferred over <c>Core.UI.Helpers.B2CTokenHelper</c> for
    /// any non-UI test: no Selenium, no browser flakiness, runs in a fraction of
    /// the time.
    /// </summary>
    public static class EntraTokenHelper
    {
        /// <summary>
        /// Signs the member in via the Entra GraphQL mutation and returns the
        /// <c>idToken</c> — the JWT downstream Virgin Atlantic APIs expect on
        /// <c>Authorization: Bearer …</c> headers. See
        /// <see href="../Tests/GraphQlTests/EntraAuthentication/EntraAuthentication.cs"/>
        /// for the reference flow (the same id-token is forwarded to
        /// <c>AccountMemberDetails</c>).
        /// </summary>
        /// <exception cref="Newtonsoft.Json.JsonException">
        ///   If the response is not in the MFA branch and is also missing the expected
        ///   <c>data.accountMemberSignIn.idToken</c> path.
        /// </exception>
        public static async Task<string> GetIdToken(string env, string email, string flyingClubNumber, string password)
        {
            string? idToken = await TryGetIdTokenAsync(env, email, flyingClubNumber, password);
            Assert.False(idToken.IsNullOrWhiteSpace(), $"Expected Flying Club member '{flyingClubNumber}' to sign in without MFA, but accountMemberSignIn required MFA.");

            return idToken ?? string.Empty;
        }

        public static async Task<string> GetIdTokenWithPasswordFallback(string env, string email, string flyingClubNumber, string password, string fallbackPassword)
        {
            try
            {
                return await GetIdToken(env, email, flyingClubNumber, password);
            }
            // Retry only the known invalid-credentials response; MFA and service failures must remain visible to the test.
            catch (Xunit.Sdk.XunitException exception) when (exception.Message.Contains("invalid username or password", StringComparison.OrdinalIgnoreCase))
            {
                return await GetIdToken(env, email, flyingClubNumber, fallbackPassword);
            }
        }

        internal static async Task<string?> TryGetIdTokenAsync(string env, string email, string flyingClubNumber, string password)
        {
            int attempt = 0;

            while (true)
            {
                attempt++;
                using HttpResponseMessage response = GraphQlEndpoint.AccountMemberSignIn(env, email, flyingClubNumber, password);

                string body = await response.Content.ReadAsStringAsync();
                // Avoid shared HTTP diagnostics here because the sign-in request includes the supplied password.
                Assert.True(response.IsSuccessStatusCode, $"accountMemberSignIn returned HTTP {(int)response.StatusCode} for Flying Club member '{flyingClubNumber}'.");

                JObject result = JObject.Parse(body);
                JToken? gqlErrors = result["errors"];
                if (gqlErrors is { HasValues: true })
                {
                    string errorCode = gqlErrors.SelectToken("[0].extensions.code")?.ToString() ?? string.Empty;
                    string errorMessage = gqlErrors.SelectToken("[0].extensions.message")?.ToString() ?? gqlErrors.SelectToken("[0].message")?.ToString() ?? "No GraphQL error message was returned.";
                    // Retry the known temporary identity-propagation response following enrolment.
                    if (errorCode.Is("601") && errorMessage.Is("Account did not meet the required status to log in") && attempt < 3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        continue;
                    }

                    Assert.Fail($"accountMemberSignIn failed for Flying Club member '{flyingClubNumber}': {errorMessage}");
                    return null;
                }

                // requiresMFA == true is a documented valid branch: in that case
                // accessToken/refreshToken/idToken are explicitly null and a
                // continuationToken is returned instead (see EntraAuthentication.cs:60-66).
                bool? requiresMfa = result.SelectToken("data.accountMemberSignIn.requiresMFA")?.Value<bool>();
                // Return no token for this documented branch so callers can safely select another test account.
                return requiresMfa == true ? null : result.RequireToken("data.accountMemberSignIn.idToken").ToString();
            }
        }
    }

}
