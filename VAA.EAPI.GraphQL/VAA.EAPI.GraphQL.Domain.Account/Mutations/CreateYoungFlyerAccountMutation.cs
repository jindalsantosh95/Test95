using HotChocolate;
using HotChocolate.Types;
using System.Globalization;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using AccountConstants = VAA.EAPI.GraphQL.Domain.Account.Constants;
using CommonApplicationConstants = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations;

[ExtendObjectType("Mutation")]
public sealed class CreateYoungFlyerAccountMutation(ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerService, IEnrolmentProvider enrolmentProvider, IHouseHoldMutationProvider houseHoldMutationProvider, YoungFlyerAccountRequestFactory youngFlyerAccountRequestFactory)
{
    private const int DefaultYoungFlyerAgeLimit = 18;

    [GraphQLName("accountCreateYoungFlyerAccount")]
    [GraphQLDescription("Creates a managed Young Flyer account and links it to the authenticated parent's household.")]
    public Task<FlyingClubSignUpResponse> CreateYoungFlyerAccount(CreateCustomerAccountRequest request) =>
        CreateYoungFlyerAccountAsync(request);

    private async Task<FlyingClubSignUpResponse> CreateYoungFlyerAccountAsync(CreateCustomerAccountRequest request)
    {
        if (request is null || request.CustomerDetails is null)
            throw CreateError(CommonApplicationConstants.MandatoryInputFieldsMissingCode, "Customer details are required.");

        var accessToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()?.AccessToken?.Token;
        if (accessToken.IsNullOrWhiteSpace())
        {
            throw CreateError(CommonApplicationConstants.MissingB2CTokenErrorCode, "An authenticated parent is required.");
        }

        var claims = Json.Deserialize<TokenB2CResponse>(await tokenB2CProvider.ValidateTokenAsync(accessToken));
        if (claims is null || claims.FcNumber.IsNullOrWhiteSpace())
        {
            throw CreateError(CommonApplicationConstants.InValidB2CTokenErrorCode, "The authenticated parent could not be resolved.");
        }

        if (!DateOnly.TryParseExact(request.CustomerDetails.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var childDateOfBirth))
        {
            throw CreateError(CommonApplicationConstants.MandatoryInputFieldsMissingCode, "The child's date of birth is invalid.");
        }

        var youngFlyerAgeLimit = ConfigReader.Try(Environment.YoungFlyerAgeLimit, DefaultYoungFlyerAgeLimit);
        if (childDateOfBirth.AddYears(youngFlyerAgeLimit) <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw CreateError(CommonApplicationConstants.MandatoryInputFieldsMissingCode, $"The child must be below the Young Flyer age limit of {youngFlyerAgeLimit}.");
        }

        // The Young Flyer token contract supplies the parent's Flying Club number. Do not
        // depend on an optional CustomerId claim when resolving the parent.
        var parent = await customerService.GetCustomer(null, claims.FcNumber);
        if (parent?.Customer is null)
        {
            throw CreateError(((int)System.Net.HttpStatusCode.InternalServerError).ToString(), "The authenticated parent customer could not be loaded.");
        }

        if (!parent.Customer.Status.Is(MemberStatus.StatusEnum.Active.ToString()))
        {
            throw CreateError(AccountConstants.Application.ParentAccountInactiveErrorCode, "The authenticated parent account is not active.");
        }

        var dummyEmail = await youngFlyerAccountRequestFactory.ApplyManagedYoungFlyerDetailsAsync(request.CustomerDetails, parent.Customer);
        request.EnrolmentToken = null;

        var enrolment = await enrolmentProvider.EnrolYoungFlyerMember(request);
        var childMemberNumber = enrolment?.MemberDetails?.MemberNumber;
        if (childMemberNumber.IsNullOrWhiteSpace() || enrolment.CustomerId.IsNullOrWhiteSpace())
        {
            throw CreateError(((int)System.Net.HttpStatusCode.InternalServerError).ToString(), "Child enrolment did not return an eligible member.");
        }

        await houseHoldMutationProvider.HouseHoldLinking(new HouseHoldLinkingRequest
        {
            InitiatedBy = claims.FcNumber,
            HeadAccount = new HouseholdAccount
            {
                CustomerId = parent.Customer.CustomerId,
                MemberId = parent.Customer.LoyaltyNumber,
                FirstName = parent.Customer.FirstName,
                LastName = parent.Customer.LastName,
                EmailId = parent.Customer.ContactDetails?.EmailDetails?.FirstOrDefault(email => email.PreferredFlag.Is("Y"))?.emailId
            },
            ChildAccount = new HouseholdAccount
            {
                CustomerId = enrolment.CustomerId,
                MemberId = childMemberNumber,
                FirstName = request.CustomerDetails.FirstName,
                LastName = request.CustomerDetails.LastName,
                EmailId = dummyEmail
            }
        });

        return enrolment;
    }

    private static GraphQLException CreateError(string code, string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .SetExtension("code", code)
            .SetExtension("message", message)
            .SetExtension("source", CommonApplicationConstants.ErrorSource)
            .Build());
}
