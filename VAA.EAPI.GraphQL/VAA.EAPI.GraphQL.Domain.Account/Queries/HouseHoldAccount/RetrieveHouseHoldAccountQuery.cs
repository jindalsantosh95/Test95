using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries.HouseHoldAccount
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class RetrieveHouseHoldAccountQuery
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ILogger<RetrieveHouseHoldAccountQuery> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IHttpConnector _connector;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<GroupDocumentDTO> _customerTransUpdateAddressHH;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;
        private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _customerTransHHInviteDocInvitee;
        private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _customerTransHHInviteDocInviter;

        private static readonly Dictionary<string, string> headersVRedMS = new()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID)}
        };

        public RetrieveHouseHoldAccountQuery(ILogger<RetrieveHouseHoldAccountQuery> logger, IAccountCustomerService customerProvider, ITokenB2CProvider<string> tokenB2CProvider, IHttpConnector connector, ICustomerTransactionQueryProvider<GroupDocumentDTO> customerTransUpdateAddressHH, IOAuthTokenProvider tokenProvider, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember, ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> customerTransHHInviteDocInvitee, ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> customerTransHHInviteDocInviter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransUpdateAddressHH = customerTransUpdateAddressHH ?? throw new ArgumentNullException(nameof(customerTransUpdateAddressHH));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
            _customerTransHHInviteDocInvitee = customerTransHHInviteDocInvitee ?? throw new ArgumentNullException(nameof(customerTransHHInviteDocInvitee));
            _customerTransHHInviteDocInviter = customerTransHHInviteDocInviter ?? throw new ArgumentNullException(nameof(customerTransHHInviteDocInviter));
        }

        [GraphQLName("retrieveHouseHoldAccount")]
        [GraphQLDescription("Retrieve HouseHold Account Query Details")]
        public async Task<RetrieveHHAccountResponse> GetRetrieveHouseHoldAccountQueryDetails(RetrieveHHAccountRequest retrieveHHAccountRequest)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var groupCustDocResponseList = new List<CustomerDTO>();
                var customerId = string.Empty;
                var customerTransactionsFlyingClubList = new List<FcMemberDetails>();
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                IEnumerable<InvitePending> inviteDocInvitee = null;
                IEnumerable<InvitePending> inviteDocInviter = null;

                var inviteeDetails = new List<CustomerDTO>();
                var inviteeTierDetails = new List<FcMemberDetails>();

                if (retrieveHHAccountRequest.MemberType == HouseHoldMemberType.Individual)
                {
                    var customerDetails = _customerProvider.GetCustomer(claim.CustomerId, claim?.FcNumber).Result.Customer;
                    var customerTierDetails = _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetFcMemberDetails}", customerDetails.CustomerId).Result;

                    Parallel.Invoke(
                            new Action(() => inviteDocInvitee = _customerTransHHInviteDocInvitee.CustomerTransactionGetInviteDocInvitee(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), claim.FcNumber).GetAwaiter().GetResult().InviteHouseHold),
                            new Action(() => inviteDocInviter = _customerTransHHInviteDocInviter.CustomerTransactionGetInviteDocInviter(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), claim.FcNumber).GetAwaiter().GetResult().InviteHouseHold));

                    if (inviteDocInvitee == null && inviteDocInviter == null)
                        return IndividualHouseHoldResponseMapper.ResponseWithNoInvites(customerDetails, customerTierDetails);

                    return await GetPendingInviteInfo(inviteDocInvitee, inviteDocInviter);
                }

                string leaderfcNumber = Convert.ToString(retrieveHHAccountRequest.MemberType) == Constants.Application.HouseHoldAccount ? claim?.FcNumber : retrieveHHAccountRequest.HouseHoldFcNumber;

                var inviteDocLeader = _customerTransHHInviteDocInviter.CustomerTransactionGetInviteDocInviter(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), claim.FcNumber).GetAwaiter().GetResult().InviteHouseHold;

                if (inviteDocLeader != null)
                    await GetInviteeDetails(inviteDocLeader, inviteeDetails, inviteeTierDetails);


                if (Convert.ToString(retrieveHHAccountRequest.MemberType) == Constants.Application.HouseHoldAccount) // leader journey
                {
                    customerId = string.IsNullOrEmpty(claim.CustomerId) ? _customerProvider.GetCustomer(claim.CustomerId, claim?.FcNumber).Result.Customer.CustomerId : claim.CustomerId;
                }

                else
                {
                    customerId = _customerProvider.GetCustomer(string.Empty, retrieveHHAccountRequest.HouseHoldFcNumber).Result.Customer.CustomerId;
                }

                var groupDocumentDTO = await GetGroupDocInfo(customerId);
                var customerIds = await GetCustomerIds(customerId, groupDocumentDTO);
                var youngFlyerCustomerIds = HouseHoldAccountMapper.GetManagedYoungFlyerCustomerIds(groupDocumentDTO);

                foreach (var custId in customerIds)
                {
                    var groupCustDocResponse = await GetCustomerDocResponse(custId, string.Empty);
                    var customerTransactionsFlyingClub = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetFcMemberDetails}", custId);

                    customerTransactionsFlyingClubList.Add(customerTransactionsFlyingClub);
                    groupCustDocResponseList.Add(groupCustDocResponse);
                }

                var totalPoints = (await _connector.SendAsync<VRedResponse>(ConfigReader.Get(Common.Constants.Environment.BookVRedVirginPoints).Replace("{memberID}", leaderfcNumber), headersVRedMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.BookVRedAppId)), HttpMethod.Get, string.Empty)).Response;

                return HouseHoldAccountMapper.RetrieveHouseHoldAccountDetails(groupCustDocResponseList, claim.FcNumber, retrieveHHAccountRequest, groupDocumentDTO.GroupDocuments.First().GroupName, leaderfcNumber, customerTransactionsFlyingClubList, totalPoints, inviteeDetails, inviteeTierDetails, youngFlyerCustomerIds);

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Login Token validation service");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Retrieve HouseHold Account Details Query failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while Retrieve HouseHold Account Details Query \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.loginValidationError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Retrieve HouseHold Account Query Details\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Error while fetching retrieve of House hold Details")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.LinkedMemberRetrievalError)
                            .Build());
            }


        }

        private async Task<CustomerDTO> GetCustomerDocResponse(string customerId, string fCNumber)
        {
            return await _customerProvider.GetCustomer(customerId, fCNumber);
        }

        private async Task<IEnumerable<string>> GetCustomerIds(string CustomerId, GroupDocumentDTO groupDocument)
        {
            return groupDocument.GroupDocuments.First().GroupHistory.Where(x => x.ExitDate == null).Select(x => x.CustomerId).Concat(new string[] { CustomerId });
        }

        private async Task<GroupDocumentDTO> GetGroupDocInfo(string CustomerId)
        {
            return await _customerTransUpdateAddressHH.CustomerTransactionUpdateAddressHouseHold(ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl), CustomerId);
        }

        private async Task<RetrieveHHAccountResponse> GetPendingInviteInfo(IEnumerable<InvitePending> inviteDocInvitee, IEnumerable<InvitePending> inviteDocInviter)
        {
            if (inviteDocInviter == null) //logged in member is invitee

                return IndividualHouseHoldResponseMapper.ResponseWithInvitesforInvitee(_customerProvider.GetCustomer(null, inviteDocInvitee.First().InviterMemberId).Result.Customer, inviteDocInvitee.First().GroupName);

            var inviteeDetails = new List<CustomerDTO>();
            var inviteeTierDetails = new List<FcMemberDetails>();

            await GetInviteeDetails(inviteDocInviter, inviteeDetails, inviteeTierDetails);

            return IndividualHouseHoldResponseMapper.ResponseWithInvitesforInviter(inviteeDetails, inviteeTierDetails, inviteDocInviter.First().GroupName);
        }

        private async Task GetInviteeDetails(IEnumerable<InvitePending> inviteDocInviter, List<CustomerDTO> inviteeDetails, List<FcMemberDetails> inviteeTierDetails)
        {
            var invitees = inviteDocInviter.Select(x => x.InviteeMemberId);

            foreach (var fcNumber in invitees)
            {
                var customerDetails = await GetCustomerDocResponse(null, fcNumber);
                var tierDetails = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetFcMemberDetails}", customerDetails.Customer.CustomerId);

                inviteeDetails.Add(customerDetails);
                inviteeTierDetails.Add(tierDetails);
            }
        }
    }
}
