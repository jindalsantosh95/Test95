using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class FlyingClubCustomerProfileProvider : IFlyingClubCustomerProfileProvider
    {
        private readonly IFlyingClubTokenProvider _tokenProvider;
        private readonly IAccountCustomerService _customerService;
        private readonly ICustomerTransactionQueryProvider<CustomerProfileDTO> _profileProvider;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _accountProvider;

        public FlyingClubCustomerProfileProvider(IFlyingClubTokenProvider tokenProvider, IAccountCustomerService customerService, ICustomerTransactionQueryProvider<CustomerProfileDTO> profileProvider, ICustomerTransactionQueryProvider<FcMemberDetails> accountProvider)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _profileProvider = profileProvider ?? throw new ArgumentNullException(nameof(profileProvider));
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
        }

        public async Task<FlyingClubCustomerProfileResponse> GetAsync(string token)
        {
            FlyingClubTokenValidationResponse validation;
            try
            {
                validation = await _tokenProvider.ValidateAsync(token);
            }
            catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new RequestValidationException("The Flying Club application token is invalid.", exception);
            }

            if (validation?.FlyingClubAccountNumber.IsNullOrWhiteSpace() ?? true)
                throw new InvalidOperationException("The validated token did not contain a Flying Club account number.");

            var customer = await _customerService.GetCustomer(null, validation.FlyingClubAccountNumber);
            if (customer?.Customer?.CustomerId.IsNullOrWhiteSpace() ?? true)
                throw new InvalidOperationException("No customer was found for the validated Flying Club account.");

            var customerTransactionsUrl = ConfigReader.Get(Environment.CustTransactionMsUrl);
            var profileTask = _profileProvider.CustomerTransactionQueryCustomerProfile(customerTransactionsUrl, customer.Customer.CustomerId);
            var accountTask = _accountProvider.CustomerTransactionQuery($"{customerTransactionsUrl}{Common.Constants.Application.GetFcMemberDetails}", customer.Customer.CustomerId);
            await Task.WhenAll(profileTask, accountTask);

            var profile = (await profileTask)?.CustomerProfile;
            var account = (await accountTask)?.FlyingClubMemberDocuments?.FirstOrDefault();
            return new FlyingClubCustomerProfileResponse
            {
                Customer = new FlyingClubCustomer
                {
                    CustomerId = customer.Customer.CustomerId,
                    LoyaltyNumber = customer.Customer.LoyaltyNumber,
                    FirstName = customer.Customer.FirstName,
                    LastName = customer.Customer.LastName,
                    DateOfBirth = customer.Customer.DateOfBirth,
                    Status = customer.Customer.Status
                },
                Profile = profile is null ? null : new FlyingClubProfile
                {
                    ReasonForTravel = profile.ReasonForTravel,
                    Brand = profile.Brand,
                    Preferences = profile.Preferences?.Select(x => new FlyingClubPreference
                    {
                        PreferenceType = x.PreferenceType,
                        PreferenceSubType = x.PreferenceSubType,
                        PreferenceValue = x.PreferenceValue
                    })
                },
                Account = account is null ? null : new FlyingClubAccount
                {
                    MemberNumber = account.MemberNumber,
                    TierCode = account.TierCode,
                    TotalTierPoints = account.TotalTierPoints,
                    VirginTierPoints = account.VsOnlyTotalTierPoints,
                    MemberType = account.MemberType,
                    HeadOfGroupMemberNumber = account.HeadOfGroupMemberNumber
                }
            };
        }
    }
}
