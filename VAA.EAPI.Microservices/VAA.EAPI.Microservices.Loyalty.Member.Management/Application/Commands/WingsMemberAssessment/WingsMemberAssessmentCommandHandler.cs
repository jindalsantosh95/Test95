using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment
{
    public class WingsMemberAssessmentCommandHandler : IWingsMemberAssessmentCommand
    {
        private readonly IReportingDBServices _reportingDBServices;
        private readonly AwardDocumentServiceQueryHandler _awardDocumentServiceQueryHandler;
        private readonly FlyingClubMemberDocumentServiceQueryHandler _flyingClubMemberDocumentServiceQueryHandler;
        private readonly CustomerTransactionDocumentsServiceQueryHandler _customerTransactionDocumentsServiceQueryHandler;
        private readonly IRulesEngineService _rulesEngineService;
        private readonly ILogger<WingsMemberAssessmentCommandHandler> _logger;

        public WingsMemberAssessmentCommandHandler(
            IReportingDBServices reportingDBServices,
            AwardDocumentServiceQueryHandler awardDocumentServiceQueryHandler,
            FlyingClubMemberDocumentServiceQueryHandler flyingClubMemberDocumentServiceQueryHandler,
            CustomerTransactionDocumentsServiceQueryHandler customerTransactionDocumentsServiceQueryHandler,
            IRulesEngineService rulesEngineService,
            ILogger<WingsMemberAssessmentCommandHandler> logger)
        {
            _reportingDBServices = reportingDBServices ?? throw new ArgumentNullException(nameof(reportingDBServices));
            _awardDocumentServiceQueryHandler = awardDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(awardDocumentServiceQueryHandler));
            _flyingClubMemberDocumentServiceQueryHandler = flyingClubMemberDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberDocumentServiceQueryHandler));
            _customerTransactionDocumentsServiceQueryHandler = customerTransactionDocumentsServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerTransactionDocumentsServiceQueryHandler));
            _rulesEngineService = rulesEngineService ?? throw new ArgumentNullException(nameof(rulesEngineService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(
            WingsMemberAssessmentCommand request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var stagingResult = await _reportingDBServices.StageGoldMemberDetailsForWingsAssessment();
                if (!stagingResult.IsSuccessful)
                {
                    throw new InvalidOperationException(
                        $"Wings assessment member staging failed: {stagingResult.StatusMessage}");
                }

                var assessedMembers = new List<GoldMemberDetails>();
                if (!int.TryParse(
                    ConfigReader.Get(Constants.Environment.WingsAssessmentBatchSize),
                    out var wingsAssessmentBatchSize) ||
                    wingsAssessmentBatchSize <= 0)
                {
                    throw new InvalidOperationException("WingsAssessmentBatchSize must be a positive integer.");
                }

                var hasMoreUnprocessedRecords = true;

                while (hasMoreUnprocessedRecords)
                {
                    var memberBatch = await _reportingDBServices.GetWingsMemberAssessmentBatch(
                        wingsAssessmentBatchSize);
                    if (memberBatch.Members.Count == 0 && memberBatch.HasMoreUnprocessedRecords)
                    {
                        throw new InvalidOperationException(
                            "Wings assessment batch retrieval returned no members while unprocessed members remain.");
                    }

                    foreach (var member in memberBatch.Members)
                    {
                        var awardDocuments = await RetrieveAwards(member, cancellationToken);
                        var wingsMemberAssessmentRequest = await ValidateFCMemberWingsEligibility(
                            member,
                            awardDocuments,
                            cancellationToken);

                        if (wingsMemberAssessmentRequest == null ||
                            wingsMemberAssessmentRequest.TierRuleSetEntityRequest == null)
                        {                           
                            continue;
                        }

                        if (!ShouldCallRulesEngine(wingsMemberAssessmentRequest.TierRuleSetEntityRequest))
                        {                           
                            continue;
                        }

                        var inRuleResponse = await _rulesEngineService.ExecuteAsync<WingsMemberAssessmentRequest, WingsMemberAssessmentInRuleResponse>(
                            Constants.Application.RuleEngineWingsMemberAssessmentEndpoint,
                            HttpMethod.Post,
                            wingsMemberAssessmentRequest,
                            cancellationToken);

                        if (inRuleResponse.HasRuntimeErrors || inRuleResponse.ResponseEntity == null)
                        {
                            throw new InvalidOperationException("Rules Engine returned an invalid Wings assessment response.");
                        }

                        if (!string.Equals(
                                inRuleResponse.ResponseEntity.CustomerId,
                                member.CustomerId,
                                StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException("Rules Engine response does not match the member being assessed.");
                        }

                        var wingsActions = inRuleResponse.ResponseEntity.RuleResponse?.Actions?.ToList();
                        if (wingsActions == null ||
                            wingsActions.Count == 0 ||
                            wingsActions.Any(action =>
                                action == null ||
                                !action.Name.Is(nameof(WingsAction)) ||
                                !Enum.TryParse<WingsAction>(action.Value, ignoreCase: false, out var parsedAction) ||
                                !Enum.IsDefined(typeof(WingsAction), parsedAction)))
                        {
                            throw new InvalidOperationException("Rules Engine returned invalid Wings assessment actions.");
                        }

                    }
                    assessedMembers.AddRange(memberBatch.Members);
                    hasMoreUnprocessedRecords = memberBatch.HasMoreUnprocessedRecords;
                }

                return $"{stagingResult.StatusMessage} Processed {assessedMembers.Count} Gold member row(s) for Wings assessment.";
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<IEnumerable<Award>> RetrieveAwards(
            GoldMemberDetails member,
            CancellationToken cancellationToken)
        {
            var awardDocumentResult = await _awardDocumentServiceQueryHandler.Handle(
                new AwardDocumentServiceQuery { CustomerId = member.CustomerId },
                cancellationToken);
            if (!awardDocumentResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to retrieve award documents for Wings assessment.");
            }

            return awardDocumentResult.Value.Value ?? Enumerable.Empty<Award>();
        }

        private async Task<WingsMemberAssessmentRequest> ValidateFCMemberWingsEligibility(
            GoldMemberDetails member,
            IEnumerable<Award> awardDocuments,
            CancellationToken cancellationToken)
        {
            var wingsAssessmentProductId = ConfigReader.Get(Constants.Environment.WingsAssessmentProductId);
            var millionMilerProductId = ConfigReader.Get(Constants.Environment.MillionMilerProductId);
            var currentDate = DateTime.UtcNow;
            var reevaluationWindowStart = currentDate.AddMonths(-12);

            var hasActiveWingsMember = awardDocuments.Any(award =>
                string.Equals(
                    award.ProductId,
                    wingsAssessmentProductId,
                    StringComparison.Ordinal) &&
                award.StartDate.HasValue &&
                award.StartDate.Value.Date >= reevaluationWindowStart.Date &&
                award.StartDate.Value.Date <= currentDate.Date &&
                string.Equals(
                    award.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase));
            var isWingsMemberEligibleForReevaluation = awardDocuments.Any(award =>
                    string.Equals(award.ProductId, wingsAssessmentProductId, StringComparison.Ordinal) &&
                    award.StartDate.HasValue &&
                    award.StartDate.Value.Date <= currentDate.Date &&
                    (string.IsNullOrWhiteSpace(award.Status) ||
                     string.Equals(award.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(award.Status, "Expired", StringComparison.OrdinalIgnoreCase)) &&
                    (award.ExpiryDate?.Date <= currentDate.Date ||
                     (!award.ExpiryDate.HasValue &&
                      award.StartDate?.Date <= reevaluationWindowStart.Date)));
            var millionMilerAward = awardDocuments
                .FirstOrDefault(award =>
                    string.Equals(award.ProductId, millionMilerProductId, StringComparison.Ordinal) &&
                    !award.ExpiryDate.HasValue &&
                    (string.IsNullOrWhiteSpace(award.Status) ||
                     string.Equals(award.Status, "Active", StringComparison.OrdinalIgnoreCase)));

            var flyingClubMemberDocumentResult = await _flyingClubMemberDocumentServiceQueryHandler.Handle(
                new FlyingClubMemberDocumentServiceQuery { CustomerId = member.CustomerId },
                cancellationToken);
            if (!flyingClubMemberDocumentResult.IsSuccess ||
                flyingClubMemberDocumentResult.Value?.Value == null)
            {
                return null;
            }

            var vsFlownMilesAttribute = flyingClubMemberDocumentResult.Value.Value?.MemberAttributes?
                .FirstOrDefault(attribute => string.Equals(
                    attribute.AttributeName,
                    Constants.Application.VSFlownMiles,
                    StringComparison.OrdinalIgnoreCase));
            var vsFlownMiles = decimal.TryParse(vsFlownMilesAttribute?.Value, out var parsedVsFlownMiles)
                ? parsedVsFlownMiles
                : 0m;
            var isMillionMilerAwardEligible = millionMilerAward != null;

            var transactionDocumentResult = await _customerTransactionDocumentsServiceQueryHandler.Handle(
                new CustomerTransactionDocumentsServiceQuery { CustomerId = member.CustomerId },
                cancellationToken);
            if (!transactionDocumentResult.IsSuccess || transactionDocumentResult.Value?.Value.Any() != true)
            {
                return null;
            }

            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
            var accrualTransList = transactionDocumentResult.Value.Value
                ?.Where(transaction =>
                    string.Equals(transaction.TransactionType, Constants.Application.TransactionType, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(transaction.StatusCode, Constants.Application.StatusCode, StringComparison.OrdinalIgnoreCase) &&
                    DateTime.TryParse(transaction.ActivityDate, out var activityDate) &&
                    activityDate >= twelveMonthsAgo && activityDate <= DateTime.UtcNow)
                .ToList() ?? new List<TransactionDocument>();
            var totalTierPoints = GetCreditPoints(accrualTransList, Constants.Application.TierPoints);
            var vsTierPoints = GetCreditPoints(accrualTransList, Constants.Application.VSTierPoints);
            var hasQualifyingVSFlight = accrualTransList.Any(transaction =>
                string.Equals(transaction.PartnerId, Constants.Application.PartnerCode, StringComparison.OrdinalIgnoreCase) &&
                transaction.CreditDetails?.Any(creditDetail =>
                    string.Equals(creditDetail.PointType, Constants.Application.TierPoints, StringComparison.OrdinalIgnoreCase) &&
                    decimal.TryParse(creditDetail.CreditValue, out var tierPoints) && tierPoints > 0) == true);
            var flyingClubMember = flyingClubMemberDocumentResult.Value.Value;
            var tierRuleSetEntityRequest = (TierRuleSetEntityRequest)new TierRuleSetEntityRequestSource
            {
                Member = member,
                FlyingClubMember = flyingClubMember,
                VSFlownMiles = vsFlownMiles,
                IsWingsMember = hasActiveWingsMember || isWingsMemberEligibleForReevaluation,
                TotalTierPoints = totalTierPoints,
                VSTierPoints = vsTierPoints,
                HasQualifyingVSFlight = hasQualifyingVSFlight,
                IsMillionMilerAwardEligible = isMillionMilerAwardEligible
            };

            return new WingsMemberAssessmentRequest
            {
                TierRuleSetEntityRequest = tierRuleSetEntityRequest
            };
        }

        private static bool ShouldCallRulesEngine(TierRuleSetEntityRequest request)
        {          
            return HasBooleanValue(request.WingsMember) &&
                   (HasPositiveValue(request.RollingPointsTotal) ||
                   HasPositiveValue(request.VSRollingTierPoints) ||
                   HasPositiveValue(request.VSFlownMiles));
        }

        private static bool HasBooleanValue(string value) =>
            value.Is(bool.TrueString) || value.Is(bool.FalseString);

        private static bool HasPositiveValue(string value)
        {
            return decimal.TryParse(
                       value,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out var parsedValue) &&
                   parsedValue > 0;
        }

        private static decimal GetCreditPoints(IEnumerable<TransactionDocument> transactions, string pointType)
        {
            return transactions
                .SelectMany(transaction => transaction.CreditDetails ?? Enumerable.Empty<Creditdetail>())
                .Where(creditDetail => string.Equals(creditDetail.PointType, pointType, StringComparison.OrdinalIgnoreCase))
                .Sum(creditDetail => decimal.TryParse(creditDetail.CreditValue, out var creditValue) ? creditValue : 0);
        }
    }
}
