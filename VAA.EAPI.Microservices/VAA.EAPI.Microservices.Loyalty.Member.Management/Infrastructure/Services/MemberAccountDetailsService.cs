using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class MemberAccountDetailsService : IMemberAccountDetailsService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MemberAccountDetailsService> _logger;
        private readonly ICachedPartners _cachedPartners;
        private readonly ICommonRefDBServices _commonRefDBServices;

        public MemberAccountDetailsService(IMediator mediator, ILogger<MemberAccountDetailsService> logger, ICachedPartners cachedPartners, ICommonRefDBServices commonRefDBServices)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedPartners = cachedPartners ?? throw new ArgumentNullException(nameof(cachedPartners));
            _commonRefDBServices = commonRefDBServices ?? throw new ArgumentNullException(nameof(commonRefDBServices));
        }
        public async Task<Result<Response<MemberAccountResponse>>> GetMemberAccountDetails(MemberAccountRequestQuery request)
        {
            try
            {
                MemberAccountResponse response = new MemberAccountResponse();
                LoyaltyMemberAccount account = new LoyaltyMemberAccount();
                account.CustomerId = request.CustomerId;
                var flyingClubMemberDocResponse = await GetFlyingClubMemberDocument(request.CustomerId);
                FlyingClubMember flyingClubMemberDoc = flyingClubMemberDocResponse.Value.Value;
                if (flyingClubMemberDoc != null)
                {
                    MemberAttribute memberAttribute = flyingClubMemberDoc?.MemberAttributes?.Where(x => x.AttributeName.ToUpper() == Constants.Application.VSFlownMiles.ToUpper()).FirstOrDefault();
                    account.TotalVsflownMiles = memberAttribute?.Value;
                    account.NumberofYearsAtGold = Convert.ToString(GetNumberOfConsecutiverYearsInGold(flyingClubMemberDoc));
                    account.GoldStartDate = flyingClubMemberDoc.GoldStartDate.HasValue ? Convert.ToDateTime(flyingClubMemberDoc.GoldStartDate) : null;
                }
                else
                {
                    return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
                IEnumerable<MembershipChange> membershipChangeDoc = await GetMembershipChangeDocument(request.CustomerId);
                IEnumerable<TransactionDocument> transactionDocuments = await GetTransactionDocuments(request.CustomerId);
                var flightProduct = await _commonRefDBServices.GetProductReferenceDataByName(Constants.Application.FlightProductName);
                var partnersResult = await _cachedPartners.GetCachedPartnersReferenceData();
                var partners = partnersResult.IsSuccess ? partnersResult.Value?.Value?.Partners : null;
                var validPartners = partners?.Where(p => p.Active && Constants.Application.PartnerCodes.Contains(p.PartnerCode)).ToList() ?? new List<PartnerProfile>();
                IEnumerable<Award> awardDoc = await GetAwardDocuments(request.CustomerId);
                if (membershipChangeDoc != null)
                {
                    var membershipChange = membershipChangeDoc.Where(w => w.Active == Constants.Application.ActiveFlag)?.FirstOrDefault();
                    account.TierStartDate = Convert.ToDateTime(membershipChange?.StartDate);
                    account.TierEndDate = Convert.ToDateTime(membershipChange?.EndDate);
                    DateTime currentPlusOneYearDate = System.DateTime.Now.AddDays(365);
                    DateTime currentPlusOneYearLastDay = new DateTime(currentPlusOneYearDate.Year, currentPlusOneYearDate.Month, 1).AddMonths(1).AddDays(-1);
                    account.ByDate = currentPlusOneYearLastDay;
                }
                if (flyingClubMemberDoc != null)
                {
                    MemberAttribute memberAttribute = flyingClubMemberDoc?.MemberAttributes?.Where(x => x.AttributeName.ToUpper() == Constants.Application.VSFlownMiles.ToUpper()).FirstOrDefault();
                    account.TotalVsflownMiles = memberAttribute?.Value;
                    account.NumberofYearsAtGold = Convert.ToString(GetNumberOfConsecutiverYearsInGold(flyingClubMemberDoc));
                }
                if (transactionDocuments != null && transactionDocuments.Any() && membershipChangeDoc != null)
                {
                    account.ByDate = CalculateMembershipExpiryDate(account.TierEndDate, transactionDocuments);
                    account.RetainPointTotal = Convert.ToString(CalculateRetainPointsTotal(transactionDocuments, account.TierStartDate, account.TierEndDate));
                    account.RollingPointTotal = Convert.ToString(CalculateRollingPointTotal(transactionDocuments));
                    account.TotalVSTierPointsInCurrentYear = Convert.ToString(CalculateVSTierPointsInCurrentMemberShipYear(transactionDocuments, account.TierStartDate, account.TierEndDate));
                    account.RetainCoreTierPoints = Convert.ToString(CalculateRetainCoreVirginTierPoints(transactionDocuments, account.TierStartDate, account.TierEndDate, validPartners, flightProduct?.ProductId ?? string.Empty));
                    account.RollingCoreTierPoints = Convert.ToString(CalculateRollingCoreTierPoints(transactionDocuments, validPartners, flightProduct?.ProductId ?? string.Empty));
                }
                if (transactionDocuments != null && transactionDocuments.Any() && flyingClubMemberDoc != null)
                {
                    account.TotalVSTierPointsSinceBecomingGold = Convert.ToString(CalculateVSTierPointsSinceGold(transactionDocuments, flyingClubMemberDoc));
                }

                if (awardDoc != null)
                {
                    var awardDocuments = awardDoc.FirstOrDefault(w => w.ProductId == request.ProductId && (w.ExpiryDate == null || w.ExpiryDate > System.DateTime.Now));
                    if (awardDocuments != null)
                    {
                        account.WingsStartDate = Convert.ToDateTime(awardDocuments?.StartDate);
                        account.TotalTierPointsSinceBecomingWings = Convert.ToString(CalculateTierPointsSinceWings(transactionDocuments, awardDocuments));
                        account.TotalVSTierPointsSinceBecomingWings = Convert.ToString(CalculateVSTierPointsSinceWings(transactionDocuments, awardDocuments));
                        account.CoreTierPointsSinceBecomingWings = Convert.ToString(CalculateCoreTierPointsSinceWings(transactionDocuments, awardDocuments, validPartners, flightProduct?.ProductId ?? string.Empty));
                    }
                }

                response.loyaltyMemberAccount = account;
                return new Response<MemberAccountResponse>
                {
                    Status = Status.OK,
                    Value = response
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message, "Fault exception from Member Document retrieve service");
                return Result.Failure<Response<MemberAccountResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
        private async Task<Result<Response<FlyingClubMember>>> GetFlyingClubMemberDocument(string customerId)
        {
            _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetFlyingClubMemberDocument - Start");
            try
            {
                var response = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = customerId });
                FlyingClubMember flyingClubMemberDoc = response.Value.Value;
                _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetFlyingClubMemberDocument - End");
                return new Response<FlyingClubMember>
                {
                    Status = Status.OK,
                    Value = flyingClubMemberDoc
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message, "Fault exception from FlyingClub Member Document retrieve service");
                return Result.Failure<Response<FlyingClubMember>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }

        private async Task<IEnumerable<MembershipChange>> GetMembershipChangeDocument(string customerId)
        {
            _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetMembershipChangeDocument - Start");
            IEnumerable<MembershipChange> membershipChangeDoc = null;
            try
            {
                var response = await _mediator.Send(request: new MembershipChangeDocumentServiceQuery { CustomerId = customerId });
                membershipChangeDoc = response.Value.Value;
                _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetMembershipChangeDocument - End");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message, "Fault exception from Membership Change Document retrieve service");
            }
            return membershipChangeDoc;
        }

        private async Task<IEnumerable<TransactionDocument>> GetTransactionDocuments(string customerId)
        {
            _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetTransactionDocuments - Start");
            IEnumerable<TransactionDocument> transactionDocuments = null;
            try
            {
                var response = await _mediator.Send(request: new CustomerTransactionDocumentsServiceQuery { CustomerId = customerId });
                transactionDocuments = response.Value.Value;
                _logger.LogInformation("MemberAccountDetailsRepoQueryHandler - GetTransactionDocuments - End");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message, "Fault exception from Transaction Documents retrieve service");
            }
            return transactionDocuments;
        }

        private int GetNumberOfConsecutiverYearsInGold(FlyingClubMember flyingClubMember)
        {
            int numberOfConsecutiverYearsInGold = 0;
            if (flyingClubMember.GoldStartDate != null && flyingClubMember.GoldStartDate <= DateTime.Now)
            {
                numberOfConsecutiverYearsInGold = (int)(System.DateTime.Now.Year - flyingClubMember?.GoldStartDate?.Year);

                if (DateTime.Now.DayOfYear < flyingClubMember?.GoldStartDate?.DayOfYear)
                {
                    numberOfConsecutiverYearsInGold = numberOfConsecutiverYearsInGold - 1;
                }
            }
            return numberOfConsecutiverYearsInGold;
        }

        private int CalculateRetainPointsTotal(IEnumerable<TransactionDocument> transactions,
               DateTime? tierStartDate, DateTime? tierEndDate)
        {
            double points = 0;
            int retainPointsTotal = 0;
            var retainPointTransaction = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
            t.TransactionType == Constants.Application.TransactionType &&
            t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(tierStartDate) &&
            Convert.ToDateTime(t.ActivityDate) <= Convert.ToDateTime(tierEndDate));
            foreach (TransactionDocument transaction in retainPointTransaction)
            {
                if (transaction?.CreditDetails != null)
                {
                    foreach (Creditdetail creditDetail in transaction?.CreditDetails)
                    {
                        if (creditDetail?.PointType == Constants.Application.TierPoints)
                        {
                            points += Convert.ToDouble(creditDetail?.CreditValue);
                        }
                    }
                }
            }
            retainPointsTotal = Convert.ToInt32(points);
            return retainPointsTotal;
        }

        private int CalculateRollingPointTotal(IEnumerable<TransactionDocument> transactions)
        {
            double points = 0;
            int rollingPointsTotal = 0;
            DateTime qualDate = DateTime.Now.AddDays(-365);
            DateTime rolQualDate = new DateTime(qualDate.Year, qualDate.Month, 1);
            IEnumerable<TransactionDocument> rollingPointTransaction = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode && t.TransactionType == Constants.Application.TransactionType && t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= rolQualDate);
            foreach (TransactionDocument txn in rollingPointTransaction)
            {
                if (txn?.CreditDetails != null)
                {
                    foreach (Creditdetail crd in txn?.CreditDetails)
                    {
                        if (crd?.PointType == Constants.Application.TierPoints)
                        {
                            points += Convert.ToDouble(crd?.CreditValue);
                        }
                    }
                }
            }
            rollingPointsTotal = Convert.ToInt32(points);
            return rollingPointsTotal;
        }

        private int CalculateVSTierPointsInCurrentMemberShipYear(IEnumerable<TransactionDocument> transactions,
            DateTime? tierStartDate, DateTime? tierEndDate)
        {
            double points = 0;
            int vsTierPointsCurrentMemYr = 0;
            var vsTierPointTransactionCurMemYr = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
            t.TransactionType == Constants.Application.TransactionType &&
            t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(tierStartDate) && Convert.ToDateTime(t.ActivityDate) <= Convert.ToDateTime(tierEndDate));

            foreach (TransactionDocument txn in vsTierPointTransactionCurMemYr)
            {
                if (txn?.CreditDetails != null)
                {
                    foreach (Creditdetail crd in txn?.CreditDetails)
                    {
                        if (crd?.PointType == Constants.Application.VSTierPoints)
                        {
                            points += Convert.ToDouble(crd?.CreditValue);
                        }
                    }
                }
            }
            vsTierPointsCurrentMemYr = Convert.ToInt32(points);
            return vsTierPointsCurrentMemYr;
        }

        private int CalculateVSTierPointsSinceGold(IEnumerable<TransactionDocument> transactions,
                FlyingClubMember flyingClubMember)
        {
            double points = 0;
            int vsTierPointsSinceGold = 0;
            if (flyingClubMember.GoldStartDate != null)
            {
                var vsTierPointTransactionSinceGold = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
                t.TransactionType == Constants.Application.TransactionType &&
                t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(flyingClubMember?.GoldStartDate));

                foreach (TransactionDocument txn in vsTierPointTransactionSinceGold)
                {
                    if (txn?.CreditDetails != null)
                    {
                        foreach (Creditdetail crd in txn?.CreditDetails)
                        {
                            if (crd?.PointType == Constants.Application.VSTierPoints)
                            {
                                points += Convert.ToDouble(crd?.CreditValue);
                            }
                        }
                    }
                }
            }
            vsTierPointsSinceGold = Convert.ToInt32(points);
            return vsTierPointsSinceGold;
        }

        private DateTime CalculateMembershipExpiryDate(DateTime? TierEndDate, IEnumerable<TransactionDocument> transactions)
        {
            DateTime qualDate = DateTime.Now.AddDays(-365);
            DateTime rolQualDate = new DateTime(qualDate.Year, qualDate.Month, 1);
            DateTime currentPlusOneYearDate = System.DateTime.Now.AddDays(365);
            DateTime currentPlusOneYearLastDay = new DateTime(currentPlusOneYearDate.Year, currentPlusOneYearDate.Month, 1).AddMonths(1).AddDays(-1);
            bool activityFound = false;
            DateTime byDate = Convert.ToDateTime(TierEndDate);
            var accrualTransactions = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
            t.TransactionType == Constants.Application.TransactionType &&
            t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= rolQualDate);
            var accrualTransactionSorted = accrualTransactions.OrderBy(t => Convert.ToDateTime(t.ActivityDate));

            foreach (TransactionDocument txn in accrualTransactionSorted)
            {
                if (txn?.CreditDetails?.Where(c => c.PointType == Constants.Application.TierPoints &&
                    Convert.ToDouble(c.CreditValue) > 0).FirstOrDefault() != null)
                {
                    DateTime activityDatePlusOneYear = Convert.ToDateTime(Convert.ToDateTime(txn?.ActivityDate).AddDays(365));
                    byDate = new DateTime(activityDatePlusOneYear.Year, activityDatePlusOneYear.Month, 1).AddMonths(1).AddDays(-1);
                    activityFound = true;
                    break;
                }
            }
            return activityFound ? byDate : currentPlusOneYearLastDay;
        }
        private async Task<IEnumerable<Award>> GetAwardDocuments(string customerId)
        {
            IEnumerable<Award> awardDocuments = null;
            try
            {
                var response = await _mediator.Send(request: new AwardDocumentServiceQuery { CustomerId = customerId });
                awardDocuments = response.Value.Value;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Fault exception from AwardDocuments retrieve service");
            }
            return awardDocuments;
        }

        private int CalculateTierPointsSinceWings(IEnumerable<TransactionDocument> transactions,
               Award award)
        {
            double points = 0;
            int tierPointsSinceWings = 0;

            if (award.StartDate != null)
            {
                var tierPointTransactionSinceWings = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
                t.TransactionType == Constants.Application.TransactionType &&
                t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(award.StartDate));

                foreach (TransactionDocument txn in tierPointTransactionSinceWings)
                {
                    if (txn?.CreditDetails != null)
                    {
                        foreach (Creditdetail crd in txn?.CreditDetails)
                        {
                            if (crd?.PointType == Constants.Application.TierPoints)
                            {
                                points += Convert.ToDouble(crd?.CreditValue);
                            }
                        }
                    }
                }
            }
            tierPointsSinceWings = Convert.ToInt32(points);
            return tierPointsSinceWings;
        }

        private int CalculateVSTierPointsSinceWings(IEnumerable<TransactionDocument> transactions, Award award)
        {
            double points = 0;
            int vsTierPointsSinceWings = 0;
            if (award.StartDate != null)
            {
                var vsTierPointTransactionSinceWings = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
                t.TransactionType == Constants.Application.TransactionType &&
                t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(award?.StartDate));

                foreach (TransactionDocument txn in vsTierPointTransactionSinceWings)
                {
                    if (txn?.CreditDetails != null)
                    {
                        foreach (Creditdetail crd in txn?.CreditDetails)
                        {
                            if (crd?.PointType == Constants.Application.VSTierPoints)
                            {
                                points += Convert.ToDouble(crd?.CreditValue);
                            }
                        }
                    }
                }
            }
            vsTierPointsSinceWings = Convert.ToInt32(points);
            return vsTierPointsSinceWings;
        }

        private int CalculateRetainCoreVirginTierPoints(IEnumerable<TransactionDocument> transactions, DateTime? tierStartDate, DateTime? tierEndDate, List<PartnerProfile> validPartners, string flightProductId)
        {
            double points = 0;
            int retainPointsTotal = 0;
            var retainPointTransaction = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
            t.TransactionType == Constants.Application.TransactionType &&
            t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(tierStartDate) &&
            Convert.ToDateTime(t.ActivityDate) <= Convert.ToDateTime(tierEndDate) && IsEligiblePartnerTransaction( t, validPartners, flightProductId));
            foreach (TransactionDocument transaction in retainPointTransaction)
            {
                if (transaction?.CreditDetails != null)
                {
                    foreach (Creditdetail creditDetail in transaction?.CreditDetails)
                    {
                        if (creditDetail?.PointType == Constants.Application.TierPoints)
                        {
                            points += Convert.ToDouble(creditDetail?.CreditValue);
                        }
                    }
                }
            }
            retainPointsTotal = Convert.ToInt32(points);
            return retainPointsTotal;
        }

        private int CalculateRollingCoreTierPoints(IEnumerable<TransactionDocument> transactions, List<PartnerProfile> validPartners, string flightProductId)
        {
            double points = 0;
            int rollingPointsTotal = 0;
            DateTime qualDate = DateTime.Now.AddDays(-365);
            DateTime rolQualDate = new DateTime(qualDate.Year, qualDate.Month, 1);
            IEnumerable<TransactionDocument> rollingPointTransaction = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode && t.TransactionType == Constants.Application.TransactionType && t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= rolQualDate && IsEligiblePartnerTransaction(t, validPartners, flightProductId));
            foreach (TransactionDocument txn in rollingPointTransaction)
            {
                if (txn?.CreditDetails != null)
                {
                    foreach (Creditdetail crd in txn?.CreditDetails)
                    {
                        if (crd?.PointType == Constants.Application.TierPoints)
                        {
                            points += Convert.ToDouble(crd?.CreditValue);
                        }
                    }
                }
            }
            rollingPointsTotal = Convert.ToInt32(points);
            return rollingPointsTotal;
        }

        private int CalculateCoreTierPointsSinceWings(IEnumerable<TransactionDocument> transactions, Award award, List<PartnerProfile> validPartners, string flightProductId)
        {
            double points = 0;
            int coreTierPointsSinceWings = 0;

            if (award.StartDate != null)
            {
                var tierPointTransactionSinceWings = transactions?.Where(t => t.StatusCode == Constants.Application.StatusCode &&
                t.TransactionType == Constants.Application.TransactionType &&
                t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= Convert.ToDateTime(award.StartDate) && IsEligiblePartnerTransaction(t, validPartners, flightProductId));

                foreach (TransactionDocument txn in tierPointTransactionSinceWings)
                {
                    if (txn?.CreditDetails != null)
                    {
                        foreach (Creditdetail crd in txn?.CreditDetails)
                        {
                            if (crd?.PointType == Constants.Application.TierPoints)
                            {
                                points += Convert.ToDouble(crd?.CreditValue);
                            }
                        }
                    }
                }
            }
            coreTierPointsSinceWings = Convert.ToInt32(points);
            return coreTierPointsSinceWings;
        }

        private bool IsEligiblePartnerTransaction(TransactionDocument transaction, List<PartnerProfile> validPartners, string flightProductId)
        {
            var partner = validPartners.FirstOrDefault(p => p.PartnerId == transaction.PartnerId);

            if (partner == null)
            {
                return false;
            }

            if (partner.PartnerCode == "VS")
            {
                return transaction.ProductId == flightProductId;
            }

            return Constants.Application.PartnerCodes.Contains(partner.PartnerCode);
        }
    }
}

