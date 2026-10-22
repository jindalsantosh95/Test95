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
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations.HouseHoldAccount
{
    [ExtendObjectType("Mutation")]
    public class InviteMemberMutation
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<InviteMemberMutation> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly IHouseHoldMutationProvider _houseHoldMutationProvider;
        private readonly ICustomerTransactionQueryProvider<string> _customerTransCreateInviteDoc;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;
        private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _customerTransHHInviteDocInvitee;
        private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _customerTransHHInviteDocInviter;
        private readonly ICustomerTransactionQueryProvider<GroupDocumentDTO> _customerTransactionQueryProviderGroupDocument;

        private readonly static Dictionary<string, string> headersmemberManagementMS = new()
        {
            { Common.Constants.Application.OcpKeyHeader,  ConfigReader.Get(Common.Constants.Environment.MemberManagementOcpKey)}
        };

        public InviteMemberMutation(ITokenB2CProvider<string> tokenB2CProvider, ILogger<InviteMemberMutation> logger, IHttpConnector httpConnector, IOAuthTokenProvider tokenProvider, IAccountCustomerService customerProvider, IHouseHoldMutationProvider houseHoldMutationProvider, ICustomerTransactionQueryProvider<string> customerTransCreateInviteDoc, ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> customerTransHHInviteDocInvitee, ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> customerTransHHInviteDocInviter, ICustomerTransactionQueryProvider<GroupDocumentDTO> customerTransactionQueryProviderGroupDoc, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransactionQueryProviderFcMemberDetails)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _houseHoldMutationProvider = houseHoldMutationProvider ?? throw new ArgumentNullException(nameof(houseHoldMutationProvider));
            _customerTransCreateInviteDoc = customerTransCreateInviteDoc ?? throw new ArgumentNullException(nameof(customerTransCreateInviteDoc));
            _customerTransFlyingClubMember = customerTransactionQueryProviderFcMemberDetails ?? throw new ArgumentNullException(nameof(customerTransactionQueryProviderFcMemberDetails));
            _customerTransHHInviteDocInvitee = customerTransHHInviteDocInvitee ?? throw new ArgumentNullException(nameof(customerTransHHInviteDocInvitee));
            _customerTransHHInviteDocInviter = customerTransHHInviteDocInviter ?? throw new ArgumentNullException(nameof(customerTransHHInviteDocInviter));
            _customerTransactionQueryProviderGroupDocument = customerTransactionQueryProviderGroupDoc ?? throw new ArgumentNullException(nameof(customerTransactionQueryProviderGroupDoc));
        }

        [GraphQLName("accountInviteMember")]
        [GraphQLDescription("Invite Member to join HouseHold Account")]
        public async Task<string> InviteMember(string inviteeFcNumber, string groupName)
        {
            var loginToken = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;

            if (loginToken.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(loginToken));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(loginToken);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var leaderCustomerTask = _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);
                var inviteDocInviteeTask = _customerTransHHInviteDocInvitee.CustomerTransactionGetInviteDocInvitee(
                    ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), inviteeFcNumber);
                var inviteDocInviterTask = _customerTransHHInviteDocInviter.CustomerTransactionGetInviteDocInviter(
                    ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), claim.FcNumber);
                var inviterDocumentTask = _customerTransHHInviteDocInviter.CustomerTransactionGetInviteDocInviter(
                    ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), inviteeFcNumber);

                await Task.WhenAll(leaderCustomerTask, inviteDocInviteeTask, inviteDocInviterTask, inviterDocumentTask);

                CustomerDTO leaderCustomerResponse = await leaderCustomerTask;
                IEnumerable<InvitePending> inviteDocInvitee = (await inviteDocInviteeTask)?.InviteHouseHold;
                IEnumerable<InvitePending> inviteDocInviter = (await inviteDocInviterTask)?.InviteHouseHold;
                IEnumerable<InvitePending> inviterDocument = (await inviterDocumentTask)?.InviteHouseHold;

                var getFlyingClubInviteMemberDetils = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetFcMemberDetails}", leaderCustomerResponse.Customer.CustomerId);

                if (getFlyingClubInviteMemberDetils.FlyingClubMemberDocuments.Any(f => !f.HeadOfGroupMemberNumber.IsNullOrEmpty()))
                {
                    var builder = ErrorBuilder.New().SetMessage(Common.Constants.Application.InviteLinkedMemberErrorMessage);
                    builder.SetExtension("code", Constants.Application.InviteLinkedMemberErrorCode);
                    builder.SetExtension("message", Common.Constants.Application.InviteLinkedMemberErrorMessage);
                    builder.SetExtension("source", Common.Constants.Application.ErrorSource);
                    throw new GraphQLException(builder.Build());
                }

                if (!inviteDocInvitee.IsNullOrEmpty() || (getFlyingClubInviteMemberDetils.FlyingClubMemberDocuments.Any(f => f.MemberType == Constants.Application.IndividualAccount) && !inviteDocInviter.IsNullOrEmpty() || !inviterDocument.IsNullOrEmpty()))
                {
                    var builder = ErrorBuilder.New().SetMessage(Common.Constants.Application.PendingInviteListMemberErrorMessage);
                    builder.SetExtension("code", Constants.Application.PendingInviteListMemberErrorCode);
                    builder.SetExtension("message", Common.Constants.Application.PendingInviteListMemberErrorMessage);
                    builder.SetExtension("source", Common.Constants.Application.ErrorSource);
                    throw new GraphQLException(builder.Build());
                }

                var getGroupDocument = await _customerTransactionQueryProviderGroupDocument.CustomerTransactionUpdateAddressHouseHold(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), leaderCustomerResponse.Customer.CustomerId);

                if (getGroupDocument.GroupDocuments.Any() && !inviteDocInviter.IsNullOrEmpty())
                {
                    var TotalHouseHoldMemberCount = getGroupDocument.GroupDocuments.First().GroupHistory.Where(g => g.ExitDate == null).Select(x => x.CustomerId).Count();
                    TotalHouseHoldMemberCount = TotalHouseHoldMemberCount + inviteDocInviter.Count() + Constants.Application.leaderFcNumberCount;
                    if (TotalHouseHoldMemberCount >= 10)
                    {
                        var builder = ErrorBuilder.New().SetMessage(Common.Constants.Application.TotalHouseHoldAccountErroMessage);
                        builder.SetExtension("code", Constants.Application.TotalHouseHoldAccountErroCode);
                        builder.SetExtension("message", Common.Constants.Application.TotalHouseHoldAccountErroMessage);
                        builder.SetExtension("source", Common.Constants.Application.ErrorSource);
                        throw new GraphQLException(builder.Build());
                    }
                }


                await _httpConnector.SendAsync<string>($"{ConfigReader.Get(Common.Constants.Environment.LoyaltyMemberManagementMsUrl)}{Common.Constants.Application.ValidateHouseholdInvitee}", headersmemberManagementMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.MemberManagementAppId)),
                        HttpMethod.Post, Json.Serialize(new HouseHoldAccountValidationRequest
                        {
                            LeaderFcNumber = claim.FcNumber,
                            IndividualFcNumber = inviteeFcNumber
                        }));

                CustomerDTO inviteeCustomerResponse = await _customerProvider.GetCustomer(null, inviteeFcNumber);

                if (inviteeCustomerResponse != null && inviteeCustomerResponse.Customer.DateOfBirth.AddYears(18) <= DateTime.Today)
                    return await _customerTransCreateInviteDoc.CustomerTransactionCreateHouseHoldInviteDoc(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), !string.IsNullOrEmpty(groupName) ? groupName : Constants.Application.DefaultHHAccountName, leaderCustomerResponse, inviteeCustomerResponse);

                HouseHoldLinkingRequest houseHoldLinkingReq = new()
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

                return (await _houseHoldMutationProvider.HouseHoldLinking(houseHoldLinkingReq)).Message;
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Invite member for Household account mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    throw new GraphQLException(errorDetails.Select(error => ErrorBuilder.New()
                        .SetMessage("Invite member for Household account mutation failed")
                        .SetExtension("code", error.Code)
                        .SetExtension("message", error.Message)
                        .SetExtension("source", error.ErrorSource)
                        .Build()).ToArray());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while sending Household invitation \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.LinkedMemberRetrievalError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Invite member for Household account mutation \r\n{ex.Message}");
                throw;
            }
        }
    }
}
