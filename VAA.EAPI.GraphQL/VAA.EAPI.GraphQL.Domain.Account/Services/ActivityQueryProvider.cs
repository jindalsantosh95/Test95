using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class ActivityQueryProvider : IActivityQueryProvider
    {
        private readonly ILogger<ActivityQueryProvider> _logger;
        private readonly IAccountCustomerService _customerProvider;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly IHttpConnector _httpConnector;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>> _customerTransTransaction;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClubMember;
        private readonly TimeProvider _timeProvider;

        public ActivityQueryProvider(ILogger<ActivityQueryProvider> logger, ITokenB2CProvider<string> tokenB2CProvider, IHttpConnector httpConnector, IAccountCustomerService customerProvider, ICustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>> customerTransTransaction, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService, IOAuthTokenProvider tokenProvider, ICustomerTransactionQueryProvider<FcMemberDetails> customerTransFlyingClubMember, TimeProvider timeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransTransaction = customerTransTransaction ?? throw new ArgumentNullException(nameof(_customerTransTransaction));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerTransFlyingClubMember = customerTransFlyingClubMember ?? throw new ArgumentNullException(nameof(customerTransFlyingClubMember));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }
        [UseSorting]
        [UseFiltering]
        public async Task<IEnumerable<ActivityDetail>> GetActivitiesQueryData(TokenB2CResponse claim, int page, int pageSize)
        {
            var customerResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);
            var customerFcmDetails = await _customerTransFlyingClubMember.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetFcMemberDetails}", customerResponse.Customer.CustomerId);

            return await GetActivitiesQueryData(claim, customerResponse.Customer, customerFcmDetails, page, pageSize);
        }

        public async Task<IEnumerable<ActivityDetail>> GetActivitiesQueryData(TokenB2CResponse claim, CustomerCustMS customer, FcMemberDetails memberDetails, int page, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(claim);
            ArgumentNullException.ThrowIfNull(customer);
            ArgumentNullException.ThrowIfNull(memberDetails);

            if (memberDetails.FlyingClubMemberDocuments.First().TierCode.Is(Application.VirginAccountTierCode))
                return null;

            var activityClaim = new TokenB2CResponse
            {
                CustomerId = customer.CustomerId,
                FcNumber = claim.FcNumber
            };
            var vredStatements = ProcessVredStatements((await GetVredStatements(activityClaim.FcNumber)).Statements);

            return ActivityResponseMapper.Mapper<ActivityDetail>(_logger, vredStatements, await GetLoopActivities(activityClaim), page, pageSize, _timeProvider);
        }

        public async Task<VredStatementResponse> GetVredStatements(string memberNumber)
        {
            var getVRedStmtsHeaders = new Dictionary<string, string>
            {
                {Application.OcpKeyHeader, ConfigReader.Get(Environment.VRedOcpSubID)}
            };

            string uri = $"{ConfigReader.Get(Environment.VredMsUrl)}{Application.GetVredStatements}".Replace("{MemberNumber}", memberNumber);

            return (await _httpConnector.SendAsync<VredStatementResponse>(uri, getVRedStmtsHeaders, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.VRedAppId)), HttpMethod.Get, string.Empty)).Response;
        }

        private static IEnumerable<Statement> ProcessVredStatements(IEnumerable<Statement> statements)
        {
            IEnumerable<Statement> vredStatements = statements
            .Where(_ => string.Equals(_.Type, Application.VredCreditTransactionType, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(_.Type, Application.VredDebitTransactionType, StringComparison.OrdinalIgnoreCase))
            .Select(statement => statement.VredDetail != null ? statement : VredNewResponse(statement))
            .Concat(statements
            .Where(_ => !string.Equals(_.Type, Application.VredCreditTransactionType, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(_.Type, Application.VredDebitTransactionType, StringComparison.OrdinalIgnoreCase))
            .Select(statement => VredNewResponse(statement)));

            return vredStatements;
        }

        private static Statement VredNewResponse(Statement statement)
        {
            return new Statement
            {
                ActivityDate = statement.ActivityDate,
                TransactionDate = statement.TransactionDate,
                Description = statement.Description,
                Type = string.Equals(statement.Type, Application.VredCreditTransactionType, StringComparison.OrdinalIgnoreCase) || string.Equals(statement.Type, Application.VredDebitTransactionType, StringComparison.OrdinalIgnoreCase) ? statement.Type : int.Parse(statement.PointsInt) < 0 ? Application.VredDebitTransactionType : Application.VredCreditTransactionType,
                PointsInt = statement.PointsInt,
                VredDetail = statement.VredDetail != null ?
                new Detail
                {
                    TransactionId = statement.VredDetail.TransactionId,
                    PartnerName = statement.VredDetail.PartnerName,
                    PartnerCode = statement.VredDetail.PartnerCode,
                    PartnerSubtype = statement.VredDetail.PartnerSubtype,
                    IsFlight = statement.VredDetail.IsFlight
                } :
                new Detail
                {
                    TransactionId = statement.Id,
                    PartnerName = Application.VRED,
                    PartnerCode = Application.VRED
                }
            };
        }

        private async Task<IEnumerable<ActivityStatementDetail>> GetLoopActivities(TokenB2CResponse claim)
        {
            if (string.IsNullOrEmpty(claim.CustomerId))
            {
                var customerResponse = await _customerProvider.GetCustomer(claim?.CustomerId, claim?.FcNumber);
                claim.CustomerId = customerResponse.Customer.CustomerId;
            }

            var customerTransResponse = await _customerTransTransaction.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Application.GetTransactionDetails}", claim.CustomerId);

            var productDetails = await _productRefService.Query();

            var partnerProfileDetails = await _partnerProfileService.Query();

            List<ActivityStatementDetail> activityStatementLoops = new();

            if (customerTransResponse != null && customerTransResponse.transactionDocuments != null)
            {
                foreach (TransactionDoc transaction in customerTransResponse?.transactionDocuments)
                {
                    var partnerProfile = partnerProfileDetails.FirstOrDefault(x => x.RowId == transaction.PartnerId);

                    var activityStatementPerTransaction = new ActivityStatementDetail
                    {
                        TransactionId = transaction?.TransactionId,
                        TransactionStatus = transaction?.StatusCode,
                        TransactionType = transaction?.TransactionType,
                        TransactionSubType = transaction?.TransactionSubType,
                        TransactionProcessedDate = string.IsNullOrEmpty(transaction.TransactionDate) ? default : Convert.ToDateTime(transaction.TransactionDate),
                        ActivityDate = string.IsNullOrEmpty(transaction.ActivityDate) ? default : Convert.ToDateTime(transaction.ActivityDate),
                        TotalPoints = transaction.TransactionValue?.ToString(),
                        ProductName = productDetails.Where(x => x.RowId == transaction.ProductId)?.Select(x => x.ProductName).FirstOrDefault(),
                        PartnerCode = partnerProfile?.PartnerCode,
                        PartnerName = partnerProfile?.Name,
                        PartnerType = partnerProfile?.PartnerType,
                        PartnerSubType = partnerProfile?.PartnerSubType,
                        Comments = transaction?.Comments,
                        StatementDescription = transaction?.StatementText,

                        Point = transaction.CreditDetails != null ? new Point()
                        {
                            CabinBonusPoints = transaction.CreditDetails.Where(c => c.PointSubType == Application.CabinBonusType).Select(c => c.CreditValue).FirstOrDefault(),
                            BasePoints = transaction.CreditDetails.Where(c => c.PointSubType == Application.BaseMilesType).Select(c => c.CreditValue).FirstOrDefault(),
                            TierBonusPoints = transaction.CreditDetails.Where(c => c.PointSubType == Application.TierBonusType).Select(c => c.CreditValue).FirstOrDefault(),
                            PromotionPoints = transaction.CreditDetails.Where(c => c.PointSubType == Application.PromotionBonusType).Select(c => c.CreditValue).FirstOrDefault(),
                            TierPoints = transaction.CreditDetails.Where(c => c.PointType == Application.TierPointsType).Select(c => c.CreditValue).FirstOrDefault()
                        } : null
                    };

                    activityStatementLoops.Add(activityStatementPerTransaction);
                }
                ;
            }

            return activityStatementLoops;
        }
    }
}
