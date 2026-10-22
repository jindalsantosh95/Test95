using HotChocolate;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class EnrolmentProvider : IEnrolmentProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly IStampingPNRsHandler _stampingPNRsHandler;

        public EnrolmentProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector, IStampingPNRsHandler stampingPNRsHandler)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _stampingPNRsHandler = stampingPNRsHandler ?? throw new ArgumentNullException(nameof(stampingPNRsHandler));
        }

        async Task<FlyingClubSignUpResponse> IEnrolmentProvider.EnrolFcMember(CreateCustomerAccountRequest request)
        {
            if (ConfigReader.Try(Environment.EnrolmentTokenFlag, false))
            {
                if (request.EnrolmentToken.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.EnrolmentToken));
                request.CustomerDetails.ContactDetails.EmailDetails.First().EmailId = (await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.OtpTokenValidation}", Helper.GetHeadersForIdentityMs(request.ChannelId), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize(new { Token = request.EnrolmentToken }))).Response;
            }

            if (request.CustomerDetails.Password != null)
            {
                var passwordErrorValidation = await _connector.SendAsync<IEnumerable<PasswordPolicy>>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidatePasswordPolicies}", Helper.GetHeadersForIdentityMs(request.ChannelId), await GetTokenForIdentityMS(), HttpMethod.Post, Json.Serialize(new PasswordValidatorRequest { Password = request.CustomerDetails.Password }));

                if (passwordErrorValidation.Response.Any(item => item.IsValid == false))
                {
                    var builder = ErrorBuilder.New().SetMessage("Enrol Flying Club SignUp request failed.");
                    builder.SetExtension("code", Application.InvalidPasswordErrorCode);
                    builder.SetExtension("message", Application.InvalidPasswordErrorMessage);
                    builder.SetExtension("source", Application.InvalidPasswordErrorsource);
                    throw new GraphQLException(builder.Build());
                }
            }

            MobileEnrolmentMarketingPreferencePolicy.Apply(request);
            var flyingClubResponse = await _connector.SendAsync<FlyingClubSignUpResponse>(ConfigReader.Get(Environment.DeDupFCGenUrl), Helper.GetHeaders(request.ChannelId), await GetToken(), HttpMethod.Post, Json.Serialize(EnrolmentEventDataMapper.CreateEnrolmentEventDataMapper(request)));

            string fcNumber = flyingClubResponse?.Response?.MemberDetails?.MemberNumber;

            if (fcNumber != null && ConfigReader.Get<bool>(Environment.AllowMemberEnrolToAzureB2C))
            {
                var b2CResponse = await _connector.SendAsync<B2CUserResponse>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.Register}", Helper.GetHeadersForIdentityMs(request.ChannelId), await GetTokenForIdentityMS(), HttpMethod.Post, Json.Serialize(EnrolMemberToB2CRequestMapper.CreateEnrolMemberToB2CRequest(flyingClubResponse?.Response?.MemberDetails?.MemberNumber, flyingClubResponse?.Response?.CustomerId, flyingClubResponse?.Response?.MemberDetails?.Status, request?.CustomerDetails)));

                if (!b2CResponse.Response.IsB2COperationSuccess)
                {
                    var builder = ErrorBuilder.New().SetMessage(b2CResponse.Response.B2CFaliureMessage);
                    builder.SetExtension("code", Application.B2COperationFailed);
                    builder.SetExtension("message", b2CResponse.Response.B2CFaliureMessage);
                    builder.SetExtension("source", Application.ErrorSource);
                    throw new GraphQLException(builder.Build());
                }
            }

            var preferences = request.CustomerDetails?.Preferences ?? Enumerable.Empty<AccountPreference>();
            bool hasFfpPreference = preferences.Any(x => x.PreferenceType == AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP && !string.IsNullOrEmpty(x.PreferenceValue));

            if (!hasFfpPreference)
                _ = _stampingPNRsHandler.StampPNRs(StampingRequestMapper.CusotmerDetailsMapper(request, fcNumber));

            return flyingClubResponse.Response;
        }

        async Task<FlyingClubSignUpResponse> IEnrolmentProvider.EnrolYoungFlyerMember(CreateCustomerAccountRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.CustomerDetails);

            // A managed child does not use the normal OTP/password path. The dedicated
            // Identity endpoint creates a disabled, communication-suppressed identity
            // for the managed child's generated dummy email and publishes the normal sync events.
            var flyingClubResponse = await _connector.SendAsync<FlyingClubSignUpResponse>(ConfigReader.Get(Environment.DeDupFCGenUrl), Helper.GetHeaders(request.ChannelId), await GetToken(), HttpMethod.Post, Json.Serialize(EnrolmentEventDataMapper.CreateEnrolmentEventDataMapper(request)));

            var response = flyingClubResponse.Response;
            if (response?.CustomerId.IsNullOrWhiteSpace() != false || response.MemberDetails?.MemberNumber.IsNullOrWhiteSpace() != false)
                throw CreateError(((int)System.Net.HttpStatusCode.InternalServerError).ToString(), "Child enrolment did not return an eligible member.");

            request.CustomerDetails.CustomerId = response?.CustomerId;
            request.CustomerDetails.FcNumber = response?.MemberDetails?.MemberNumber;
            request.CustomerDetails.Status = MemberStatus.StatusEnum.Active.ToString();
            response.MemberDetails.Status = MemberStatus.StatusEnum.Active.ToString();

            var identityResponse = await _connector.SendAsync<B2CUserResponse>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.RegisterYoungFlyer}", Helper.GetHeadersForIdentityMs(request.ChannelId), await GetTokenForIdentityMS(), HttpMethod.Post, Json.Serialize(new { request.CustomerDetails }));

            if (identityResponse?.Response?.IsB2COperationSuccess != true)
                throw CreateError(Application.B2COperationFailed, identityResponse?.Response?.B2CFaliureMessage ?? "Young Flyer B2C registration failed.");

            return response;
        }

        async Task<EntraEnrolTokenResponse> IEnrolmentProvider.EntraEnrolFcMember(CreateCustomerAccountRequest request)
        {
            MobileEnrolmentMarketingPreferencePolicy.Apply(request);
            var flyingClubResponse = await _connector.SendAsync<FlyingClubSignUpResponse>(ConfigReader.Get(Environment.DeDupFCGenUrl), Helper.GetHeaders(request.ChannelId), await GetToken(), HttpMethod.Post, Json.Serialize(EnrolmentEventDataMapper.CreateEnrolmentEventDataMapper(request)));

            string fcNumber = flyingClubResponse?.Response?.MemberDetails?.MemberNumber;

            var response = await _connector.SendAsync<TokenResponse>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.CompleteSignUp}", Helper.GetHeadersForIdentityMs(request.ChannelId), await GetTokenForIdentityMS(), HttpMethod.Post, Json.Serialize(EnrolMemberToB2CRequestMapper.CreateEntraEnrolMemberToB2CRequest(flyingClubResponse?.Response?.MemberDetails?.MemberNumber, flyingClubResponse?.Response?.CustomerId, flyingClubResponse?.Response?.MemberDetails?.Status, request?.CustomerDetails, request?.EnrolmentToken)));

            if (response?.Response == null)
                throw new InvalidOperationException("CompleteSignUp returned an empty token response.");

            var preferences = request.CustomerDetails?.Preferences ?? [];
            bool hasFfpPreference = preferences.Any(x => x.PreferenceType == AccountPreference.AccountPreferenceTypes.PreferredPartnerFFP && !string.IsNullOrEmpty(x.PreferenceValue));

            if (!hasFfpPreference)
                _ = _stampingPNRsHandler.StampPNRs(StampingRequestMapper.CusotmerDetailsMapper(request, fcNumber));

            return new EntraEnrolTokenResponse
            {
                AccessToken = response.Response.AccessToken,
                ContinuationToken = response.Response.ContinuationToken,
                ExpiresIn = response.Response.ExpiresIn,
                IdToken = response.Response.IdToken,
                RefreshToken = response.Response.RefreshToken,
                Scope = response.Response.Scope,
                TokenType = response.Response.TokenType,
                MemberNumber = fcNumber,
                FirstName = request.CustomerDetails.FirstName,
                LastName = request.CustomerDetails.LastName
            };
        }


        private async Task<string> GetToken()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustomerAppId));
        }

        private async Task<string> GetTokenForIdentityMS()
        {
            return await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));
        }

        private static GraphQLException CreateError(string code, string message) =>
            new(ErrorBuilder.New()
                .SetMessage(message)
                .SetCode(code)
                .SetExtension("code", code)
                .SetExtension("message", message)
                .SetExtension("source", Application.ErrorSource)
                .Build());
    }
}
