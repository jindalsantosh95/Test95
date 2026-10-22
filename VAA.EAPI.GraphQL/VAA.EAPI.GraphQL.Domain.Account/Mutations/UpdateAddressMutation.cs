using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UpdateAddressMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<UpdateAddressMutation> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<GroupDocumentDTO> _customerTransUpdateAddressHH;
        private readonly IRefDataService<CountryRefData> _countriesRefService;

        private readonly static Dictionary<string, string> headersCustomerMS = new Dictionary<string, string>()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CustomerOcpKey)}
        };

        public UpdateAddressMutation(ITokenB2CProvider<string> tokenB2CProvider, ILogger<UpdateAddressMutation> logger, IHttpConnector httpConnector, IOAuthTokenProvider tokenProvider, IAccountCustomerService customerProvider, ICustomerTransactionQueryProvider<GroupDocumentDTO> customerTransUpdateAddressHH, IRefDataService<CountryRefData> countriesRefService)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransUpdateAddressHH = customerTransUpdateAddressHH ?? throw new ArgumentNullException(nameof(customerTransUpdateAddressHH));
            _countriesRefService = countriesRefService ?? throw new ArgumentNullException(nameof(countriesRefService));
        }

        [GraphQLName("accountUpdateAddressDetails")]
        [GraphQLDescription("Update Customer's Address through Manage Account")]
        public async Task<string> UpdateMemberAddress(UpdateMemberAddressRequest updateAddressDetail)
        {
            var countryList = ConfigReader.Get(Common.Constants.Environment.PostCodeMandatoryCountryList).Split("|");

            if (countryList.Any(country => string.Equals(country, updateAddressDetail.AccountAddressDetail.Country, StringComparison.OrdinalIgnoreCase)) && string.IsNullOrEmpty(updateAddressDetail.AccountAddressDetail.PostalCode))
            {
                var builder = ErrorBuilder.New().SetMessage("PostalCode Missing");

                builder.SetExtension("code", Application.MandatoryInputFieldsMissingCode);
                builder.SetExtension("message", Application.MandatoryInputFieldsMissingMessage);
                builder.SetExtension("source", Application.MandatoryInputFieldsMissingErrorsource);

                throw new GraphQLException(builder.Build());
            }

            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerPersonalResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);

                var countryDetails = await _countriesRefService.Query();

                updateAddressDetail.AccountAddressDetail.Country = countryDetails.First(x => x.IATACountryCode.Is(updateAddressDetail.AccountAddressDetail.Country)).CountryName;

                if (updateAddressDetail.MemberType == HouseHoldMemberType.Individual)

                    return (await _httpConnector.SendAsync<ManageAccountUpdateResponse>(ConfigReader.Get(Common.Constants.Environment.CustomerMSUrlForUpdateMemberAddress).Replace("{CustomerId}", customerPersonalResponse.Customer.CustomerId), headersCustomerMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)),
                        HttpMethod.Patch, Json.Serialize(new UpdateAddressRequest
                        {
                            CustomerIds = null,
                            NewAddressDetails = updateAddressDetail.AccountAddressDetail
                        }))).Response.Value;


                var groupDocument = await _customerTransUpdateAddressHH.CustomerTransactionUpdateAddressHouseHold(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), customerPersonalResponse.Customer.CustomerId);

                IEnumerable<string> customerIds = groupDocument.GroupDocuments.First().GroupHistory.Where(x => x.ExitDate == null).Select(x => x.CustomerId);

                return (await _httpConnector.SendAsync<ManageAccountUpdateResponse>(ConfigReader.Get(Common.Constants.Environment.CustomerMSUrlForUpdateMemberAddress).Replace("{CustomerId}", customerPersonalResponse.Customer.CustomerId), headersCustomerMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)),
                        HttpMethod.Patch, Json.Serialize(new UpdateAddressRequest
                        {
                            CustomerIds = customerIds,
                            NewAddressDetails = updateAddressDetail.AccountAddressDetail
                        }))).Response.Value;
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Update Member Address mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Address Update failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Address for Member \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberAddressError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Update Member Address mutation \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Address for Member")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberAddressError)
                            .Build());
            }
        }
    }
}
