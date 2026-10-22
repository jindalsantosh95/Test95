using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations.HouseHoldAccount
{
    [ExtendObjectType("Mutation")]
    public class InviteActionMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<InviteActionMutation> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<string> _customerTransDeleteInviteDoc;
        private readonly IHouseHoldMutationProvider _houseHoldMutationProvider;

        private readonly static Dictionary<string, string> headersmemberManagementMS = new()
        {
            { Common.Constants.Application.OcpKeyHeader,  ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey)}
        };

        public InviteActionMutation(ITokenB2CProvider<string> tokenB2CProvider, ILogger<InviteActionMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, IAccountCustomerService customerProvider, ICustomerTransactionQueryProvider<string> customerTransDeleteInviteDoc, IHouseHoldMutationProvider houseHoldMutationProvider)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransDeleteInviteDoc = customerTransDeleteInviteDoc ?? throw new ArgumentNullException(nameof(customerTransDeleteInviteDoc));
            _houseHoldMutationProvider = houseHoldMutationProvider ?? throw new ArgumentNullException(nameof(houseHoldMutationProvider));
        }


        [GraphQLName("accountInviteMemberAction")]
        [GraphQLDescription("Accept, Decline or Cancel HouseHold Account Invitation.")]
        public async Task<string> HouseHoldInviteAction(HouseHoldInviteActionRequest request)
        {
            var loginToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;

            if (loginToken.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(loginToken));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(loginToken);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                if (request.ActionType == InviteActionType.Accepted)
                {
                    await _httpConnector.SendAsync<string>($"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.ValidateHouseholdInvitee}", headersmemberManagementMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId)),
                            HttpMethod.Post, Json.Serialize(new HouseHoldAccountValidationRequest
                            {
                                LeaderFcNumber = request.InviterFcNumber,
                                IndividualFcNumber = claim.FcNumber
                            }));

                    await _customerTransDeleteInviteDoc.CustomerTransactionDeleteHouseHoldInviteDoc(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), request.InviterFcNumber, claim.FcNumber);

                    await _houseHoldMutationProvider.HouseHoldLinking(HouseHoldLinking(request.InviterFcNumber, claim.FcNumber));
                }

                if (request.ActionType == InviteActionType.Cancelled || request.ActionType == InviteActionType.Declined)
                {
                    await _customerTransDeleteInviteDoc.CustomerTransactionDeleteHouseHoldInviteDoc(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), request.InviterFcNumber, request.InviteeFcNumber);
                }

                return $"Successfully {request.ActionType} HouseHold Invite";
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HouseHold Invite Action mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("HouseHold Invite Action mutation failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error While processing HouseHold Invite Action\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.LinkedMemberRetrievalError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Invite member Action for Household account mutation \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while processing HouseHold Invitaion Action")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.LinkedMemberRetrievalError)
                            .Build());
            }
        }

        private HouseHoldLinkingRequest HouseHoldLinking(string leaderFcNumber, string inviteeFcNumber)
        {
            CustomerDTO inviteeCustomerResponse = new();
            CustomerDTO leaderCustomerResponse = new();

            Parallel.Invoke(
                new Action(() => inviteeCustomerResponse = _customerProvider.GetCustomer(null, inviteeFcNumber).GetAwaiter().GetResult()),
                new Action(() => leaderCustomerResponse = _customerProvider.GetCustomer(null, leaderFcNumber).GetAwaiter().GetResult())
                );

            return new HouseHoldLinkingRequest()
            {
                InitiatedBy = leaderCustomerResponse.Customer.LoyaltyNumber,
                HeadAccount = new HouseholdAccount()
                {
                    CustomerId = leaderCustomerResponse.Customer.CustomerId,
                    MemberId = leaderCustomerResponse.Customer.LoyaltyNumber,
                    FirstName = leaderCustomerResponse.Customer.FirstName,
                    LastName = leaderCustomerResponse.Customer.LastName,
                    EmailId = leaderCustomerResponse.Customer.ContactDetails?.EmailDetails?.First(x => x.PreferredFlag == "Y").emailId
                },
                ChildAccount = new HouseholdAccount()
                {
                    CustomerId = inviteeCustomerResponse.Customer.CustomerId,
                    MemberId = inviteeCustomerResponse.Customer.LoyaltyNumber,
                    FirstName = inviteeCustomerResponse.Customer.FirstName,
                    LastName = inviteeCustomerResponse.Customer.LastName,
                    EmailId = inviteeCustomerResponse.Customer.ContactDetails?.EmailDetails?.First(x => x.PreferredFlag == "Y").emailId
                }
            };
        }
    }
}
