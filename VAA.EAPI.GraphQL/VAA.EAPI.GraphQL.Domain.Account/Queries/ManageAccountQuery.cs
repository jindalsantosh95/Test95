using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ManageAccountQuery
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ILogger<ManageAccountQuery> _logger;
        private readonly ICustomerTransactionQueryProvider<CustomerProfileDTO> _customerTransCustomerProfile;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;
        private readonly IRefDataService<CountryRefData> _countriesRefService;

        public ManageAccountQuery(ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider, ILogger<ManageAccountQuery> logger, ICustomerTransactionQueryProvider<CustomerProfileDTO> customerTransCustomerProfile, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember, IRefDataService<CountryRefData> countriesRefService)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerTransCustomerProfile = customerTransCustomerProfile ?? throw new ArgumentNullException(nameof(customerTransCustomerProfile));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
            _countriesRefService = countriesRefService ?? throw new ArgumentNullException(nameof(countriesRefService));
        }

        [GraphQLName("accountManageAccount")]
        [GraphQLDescription("Retrieve Basic Information of Customer for Updation.")]
        public async Task<ManageAccountResponse<AddressCustMS>> ManageAccount()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);

                var customerTransactions = await _customerTransCustomerProfile.CustomerTransactionQueryCustomerProfile(ConfigReader.Get(Environment.CustTransactionMsUrl), customerResponse.Customer.CustomerId);

                var customerTransactionsFlyingClub = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetFcMemberDetails}", customerResponse.Customer.CustomerId);

                var countryDetails = await _countriesRefService.Query();

                var primaryAddress = customerResponse.Customer?.AddressDetails?.Where(x => x.PreferredFlag == "Y");

                if (primaryAddress != null && primaryAddress.Any())
                    primaryAddress.First().CountryCode = countryDetails.First(x => x.CountryName.Is(primaryAddress?.First().Country)).IATACountryCode;

                else
                    primaryAddress = new List<AddressCustMS>()
                    { new()};

                return new ManageAccountResponse<AddressCustMS>()
                {
                    FirstName = customerResponse.Customer?.FirstName,
                    LastName = customerResponse.Customer?.LastName,
                    DateofBirth = customerResponse.Customer.DateOfBirth,
                    Gender = customerResponse.Customer?.Gender,
                    Email = customerResponse.Customer?.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.emailId,
                    ContactNumber = customerResponse.Customer?.ContactDetails?.PhoneDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.PhoneNumber,
                    CountryCode = customerResponse.Customer?.ContactDetails?.PhoneDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.CountryCode,
                    Address = primaryAddress,
                    IsVAAMarketingPreferred = customerTransactions?.CustomerProfile?.Preferences?.FirstOrDefault(x => x.PreferenceType == Common.Constants.Application.PrefenceType)?.PreferenceValue == "Y",
                    MemberType = GetMemberType(customerTransactionsFlyingClub),
                    HeadOfGroupMemberNumber = customerTransactionsFlyingClub.FlyingClubMemberDocuments.First().HeadOfGroupMemberNumber
                };
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Login Token validation service");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Member Account Query  failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Member Account query\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberDetailsError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception in Member Account query");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberDetailsError)
                            .Build());
            }
        }

        private static HouseHoldMemberType GetMemberType(FcMemberDetails customerTransactionsFlyingClub)
        {
            switch (customerTransactionsFlyingClub.FlyingClubMemberDocuments.FirstOrDefault()?.MemberType.ToLower())
            {
                case "household":
                    return HouseHoldMemberType.Household;

                case "individual":
                    if (!string.IsNullOrEmpty(customerTransactionsFlyingClub.FlyingClubMemberDocuments.FirstOrDefault()?.HeadOfGroupMemberNumber))
                        return HouseHoldMemberType.Linked;
                    break;
            }

            return HouseHoldMemberType.Individual;
        }
    }
}
