using HotChocolate;
using HotChocolate.Types;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class VirginAccountUpgradeMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerMutationProvider<string> _customerMutationProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly IStampingPNRsHandler _stampingPNRsHandler;


        public VirginAccountUpgradeMutation(ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider, ICustomerMutationProvider<string> customerMutationProvider, IHttpConnector httpConnector, IStampingPNRsHandler stampingPNRsHandler)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerMutationProvider = customerMutationProvider ?? throw new ArgumentNullException(nameof(customerMutationProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _stampingPNRsHandler = stampingPNRsHandler ?? throw new ArgumentNullException(nameof(stampingPNRsHandler));
        }


        [GraphQLName("virginAccountUpgrade")]
        [GraphQLDescription("Upgrade Enrols a Flying club member into loyalty program.")]
        public async Task<string> upgradeVirginAccount(VirginAccountDetails virginAccountDetails)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);

                var metroDetails = (await _httpConnector.GetAsync<IEnumerable<CountryDetails>>(ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl).Replace("{0}", Common.Constants.Application.GetCountryDetails), null, string.Empty)).Response;

                await _customerMutationProvider.UpgradeFlyingClub(EnrolmentEventDataMapper.CreateEnrolmentDataMapper(customerResponse.Customer, virginAccountDetails, metroDetails));

                //Add FcNumber to customer's future Bookings
                _stampingPNRsHandler.StampPNRs(customerResponse);

                return "Account Upgraded Successfully.";

            }
            catch (HttpRequestException ex)
            {
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Upgrade of Virgin Account failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in VirginAccountUpgrade\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
        }
    }
}
