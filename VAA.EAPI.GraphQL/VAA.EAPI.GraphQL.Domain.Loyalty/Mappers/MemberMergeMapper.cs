using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class MemberMergeMapper
    {
        public static MemberMergeResponse MapToMemberMergeResponse(IEnumerable<ErrorDetails> errors, MemberMergeMutationRequest request)
        {
            if (errors is not null)
            {
                return new MemberMergeResponse()
                {
                    Errors = new List<ErrorDetails>()
                    {
                         new ErrorDetails()
                         {
                             Code = errors.First().Code,
                             Message= errors.First().Message,
                             DeveloperMessage= errors.First().DeveloperMessage
                         }
                    }
                };
            }

            return new MemberMergeResponse()
            {
                MemberNumber = request.MergedMemberNumber,
                MergeToMemberNumber = request.SurvivorMemberNumber,
                Value = "Merge operation completed successfully"
            };
        }

        public static VredSyncTransactionRequest MapToVredTransactionRequest(MemberMergeMutationRequest memberMergeRequest, ProductDetails productDetails, PartnerProfileDetails partnerDetails, string usablePoints, CustomerCustMS survivorMemberDetails)
        {
            if (memberMergeRequest is null) throw new ArgumentNullException(nameof(memberMergeRequest));
            if (productDetails is null) throw new ArgumentNullException(nameof(productDetails));
            if (partnerDetails is null) throw new ArgumentNullException(nameof(partnerDetails));

            return new VredSyncTransactionRequest()
            {
                CustomerId = survivorMemberDetails != null ? survivorMemberDetails.CustomerId : memberMergeRequest.MergedCustomerId,
                MemberNumber = survivorMemberDetails != null ? survivorMemberDetails.LoyaltyNumber : memberMergeRequest.MergedMemberNumber,
                PartnerCode = partnerDetails.PartnerCode,
                ProductName = Common.Constants.Application.VredPointsTransferProductName,
                ActivityDate = DateTime.UtcNow,
                Points = usablePoints.Parse(0),
                PointType = !string.IsNullOrEmpty(usablePoints) ? Common.Constants.Application.PointType : null,
                TransactionId = Utility.GetId(),
                TransactionDate = DateTime.UtcNow,
                TransactionNumber = Utility.GetId(),
                PartnerName = partnerDetails?.Name,
                PartnerCategory = partnerDetails?.PartnerType,
                ProductDescription = productDetails?.Description,
                TransactionSubType = Common.Constants.Application.TxnSubType,
                TransactionChannel = Common.Constants.Application.PAUser,
                ActivityType = survivorMemberDetails != null ? Common.Constants.Application.AccrualTransactionType : Common.Constants.Application.RedemptionTransactionType,
                Status = Common.Constants.Application.TxnProcessingStatus,
                CreatedBy = memberMergeRequest.InitiatedBy,
                UpdatedBy = memberMergeRequest.InitiatedBy,
                ProductCategory = productDetails?.ProductCategory,
                TransferMemberNumber = survivorMemberDetails != null ? string.Empty : memberMergeRequest.SurvivorMemberNumber,
                StatementText = string.Empty
            };
        }

        public static TransactionStatementTextRequest MapToTransactionStatementTextRequest(MemberMergeMutationRequest memberMergeRequest, ProductDetails product, PartnerProfileDetails partner, string ActivityType, string MemberAccountType)
        {
            if (memberMergeRequest is null) throw new ArgumentNullException(nameof(memberMergeRequest));
            if (product is null) throw new ArgumentNullException(nameof(product));
            if (partner is null) throw new ArgumentNullException(nameof(partner));

            return new TransactionStatementTextRequest()
            {
                MemberNumber = memberMergeRequest.MergedMemberNumber,
                TransactionType = ActivityType,
                ProductName = product.ProductName,
                ProductDescription = product.Description,
                PartnerName = partner.Name,
                ActivityNumber = string.Empty,
                PartnerCode = partner.PartnerCode,
                TransactionSubType = Common.Constants.Application.TxnSubType,
                MemberType = MemberAccountType,
                MemberMediaCode = string.Empty,
                MergedMemberNumber = memberMergeRequest.SurvivorMemberNumber,
                StatementText = string.Empty,
                VoucherName = product.ProductName
            };
        }

        public static FlyingClubMemberRequest MapToFCMRequest(MemberMergeMutationRequest memberMergeRequest, CustomerCustMS survivorMemberDetails, FlyingClubMember survivorAccFlyingClubDoc, string miles)
        {
            if (memberMergeRequest is null) throw new ArgumentNullException(nameof(memberMergeRequest));
            if (survivorMemberDetails is null) throw new ArgumentNullException(nameof(survivorMemberDetails));

            FlyingClubMember flyingClubMemberDoc = new();
            if (string.IsNullOrEmpty(miles))
            {
                flyingClubMemberDoc.CustomerId = memberMergeRequest.MergedCustomerId;
                flyingClubMemberDoc.Status = Common.Constants.Application.MergedFCMDocStatus;
                flyingClubMemberDoc.MergeToMemberNumber = survivorMemberDetails.LoyaltyNumber;
                flyingClubMemberDoc.LastUpdatedBy = memberMergeRequest.InitiatedBy;
            }
            else
            {
                IList<MemberAttribute> attributeList = new List<MemberAttribute>();
                MemberAttribute vsFlownMiles = survivorAccFlyingClubDoc.MemberAttributes?.Where(x => x.AttributeName.Is(Common.Constants.Application.VSFlownMiles))?.FirstOrDefault();
                if (vsFlownMiles != null)
                {
                    vsFlownMiles.Value = Convert.ToString(Convert.ToInt32(vsFlownMiles.Value) + Convert.ToInt32(miles));
                    vsFlownMiles.LastUpdatedBy = memberMergeRequest.InitiatedBy;
                    attributeList.Add(vsFlownMiles);
                }
                else
                {
                    attributeList.Add(
                        new MemberAttribute
                        {
                            CreatedBy = memberMergeRequest.InitiatedBy,
                            LastUpdatedBy = memberMergeRequest.InitiatedBy,
                            AttributeName = Common.Constants.Application.VSFlownMiles,
                            Value = miles ?? string.Empty
                        });
                }
                flyingClubMemberDoc.CustomerId = memberMergeRequest.SurvivorCustomerId;
                flyingClubMemberDoc.MemberAttributes = attributeList;
                flyingClubMemberDoc.LastUpdatedBy = memberMergeRequest.InitiatedBy;
            }

            FlyingClubMemberRequest request = new()
            {
                FlyingClubMemberDocument = flyingClubMemberDoc
            };

            return request;
        }

        public static AwardRequest MapToAwardRequest(MemberMergeMutationRequest memberMergeRequest, CustomerCustMS memberDetails, Models.Award award)
        {
            if (award is null) throw new ArgumentNullException(nameof(award));

            return new AwardRequest()
            {
                award = new Models.Award
                {
                    id = memberMergeRequest.SurvivorMemberNumber != memberDetails.LoyaltyNumber ? award.id : string.Empty,
                    AwardNumber = award.AwardNumber,
                    ProductId = award.ProductId,
                    StartDate = award.StartDate,
                    ExpiryDate = award.ExpiryDate,
                    TransactionId = award.TransactionId,
                    AwardBasis = award.AwardBasis,
                    AwardId = award.AwardId,
                    AwardName = award.AwardName,
                    Status = memberMergeRequest.SurvivorMemberNumber != memberDetails.LoyaltyNumber ? AwardStatusEnum.Cancelled.ToString() : award.Status,
                    ExpiryDateUpdatedBy = award.ExpiryDateUpdatedBy,
                    SourceIdentifier = award.SourceIdentifier,
                    SourceSystem = award.SourceSystem,
                    GiftedAwardDate = award.GiftedAwardDate,
                    GiftedFromCustomerId = award.GiftedFromCustomerId,
                    GiftedToCustomerId = award.GiftedToCustomerId,
                    PreviousStatus = award.PreviousStatus,
                    CreatedBy = memberMergeRequest.SurvivorMemberNumber != memberDetails.LoyaltyNumber ? award.CreatedBy : memberMergeRequest.InitiatedBy,
                    CreatedOn = memberMergeRequest.SurvivorMemberNumber != memberDetails.LoyaltyNumber ? award.CreatedOn : null,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    CustomerId = memberDetails.CustomerId
                }
            };
        }

        public static TransactionRequest MapToTransactionRequest(VredSyncTransactionRequest VredTransactionRequest, MemberMergeMutationRequest memberMergeRequest, string productId, PartnerProfileDetails partnerDetails, string miles, string VSTierPoints, string nonVSTierPoints, string totalPoints, string usablePoints, string statementText, string transactionType)
        {
            if (memberMergeRequest is null) throw new ArgumentNullException(nameof(memberMergeRequest));
            if (productId is null) throw new ArgumentNullException(nameof(productId));
            if (partnerDetails is null) throw new ArgumentNullException(nameof(partnerDetails));

            DateTime dateTimeNow = DateTime.UtcNow;
            List<CreditDetails> creditDetails = new List<CreditDetails>();

            if (!string.IsNullOrEmpty(VSTierPoints) && Convert.ToInt32(VSTierPoints) > 0)
            {
                CreditDetails creditPoints = new()
                {
                    CreatedBy = memberMergeRequest.InitiatedBy,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy,
                    CreditValue = VSTierPoints,
                    PointType = Common.Constants.Application.VSTierPoints,
                    PointSubType = "",
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlagForTierPoints
                };
                creditDetails.Add(creditPoints);
            }

            if (!string.IsNullOrEmpty(nonVSTierPoints) && Convert.ToInt32(nonVSTierPoints) > 0)
            {
                CreditDetails creditNonVSPoints = new()
                {
                    CreatedBy = memberMergeRequest.InitiatedBy,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy,
                    CreditValue = nonVSTierPoints,
                    PointType = Common.Constants.Application.TierPointsType,
                    PointSubType = "",
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlagForTierPoints
                };
                creditDetails.Add(creditNonVSPoints);
            }

            if (!string.IsNullOrEmpty(miles) && Convert.ToInt32(miles) > 0)
            {
                CreditDetails creditMiles = new()
                {
                    CreatedBy = memberMergeRequest.InitiatedBy,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy,
                    CreditValue = miles,
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.BaseMilesType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag
                };
                creditDetails.Add(creditMiles);
            }

            return new()
            {
                TransactionDocument = new()
                {
                    TransactionId = VredTransactionRequest?.TransactionId ?? Utility.GetId(),
                    CreatedOn = dateTimeNow,
                    CreatedBy = VredTransactionRequest?.CreatedBy ?? memberMergeRequest.InitiatedBy,
                    LastUpdatedBy = VredTransactionRequest?.UpdatedBy ?? memberMergeRequest.InitiatedBy,
                    LastUpdatedOn = dateTimeNow,
                    CustomerId = VredTransactionRequest?.CustomerId ?? memberMergeRequest.SurvivorCustomerId,
                    MemberNumber = VredTransactionRequest?.MemberNumber ?? memberMergeRequest.SurvivorMemberNumber,
                    TransactionValue = usablePoints,
                    PartnerId = partnerDetails.RowId,
                    TransactionDate = dateTimeNow,
                    ProductId = productId,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionChannel = Common.Constants.Application.PAUser,
                    ActivityDate = dateTimeNow,
                    TransactionType = transactionType,
                    TierPointsToCredit = totalPoints,
                    ProcessingComment = string.Empty,
                    StatementText = statementText,
                    FinancialCreditDetails = new FinancialCreditDetails(),
                    TransactionNotes = null,
                    CreditDetails = creditDetails,
                    FlightDetails = new FlightDetails()
                }
            };
        }

        public static PartnerMembershipRequest MapToCenturionCardRequest(MemberMergeMutationRequest memberMergeRequest, CustomerCustMS MemberDetails, PartnerMembership partnerMembership, HttpMethod httpMethodType)
        {
            return new PartnerMembershipRequest()
            {
                PartnerMembershipDocument = new PartnerMembership
                {
                    Id = httpMethodType == HttpMethod.Post ? string.Empty : partnerMembership.Id,
                    CustomerId = MemberDetails.CustomerId,
                    PartnerId = partnerMembership.PartnerId,
                    PartnerCode = partnerMembership.PartnerCode,
                    PartnerStatusCode = partnerMembership.PartnerStatusCode,
                    Description = (partnerMembership.PartnerCode == Common.Constants.Application.CenturionPartnerCode) ? Common.Constants.Application.CenturionDescription : string.Empty,
                    StartDate = httpMethodType == HttpMethod.Post ? DateTime.UtcNow : partnerMembership.StartDate,
                    MemberNumber = MemberDetails.LoyaltyNumber,
                    CreatedBy = httpMethodType == HttpMethod.Post ? memberMergeRequest.InitiatedBy : partnerMembership.CreatedBy,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy,
                    EndDate = httpMethodType == HttpMethod.Post ? null : DateTime.UtcNow
                },
                RequestType = httpMethodType
            };
        }

        public static string CalculateTierPointsInLastOneYear(IEnumerable<Transaction> transactions, string TierPointsType)
        {
            double TierPoints = 0;
            var TierPointTransactionInLastOneYr = transactions.Where(t => t.StatusCode == Common.Constants.Application.TxnProcessingStatus &&
            t.TransactionType == Common.Constants.Application.AccrualTransactionType &&
            t.ActivityDate != null && Convert.ToDateTime(t.ActivityDate) >= DateTime.UtcNow.AddMonths(Common.Constants.Application.TierPointsMonthsValidity));

            foreach (Transaction txn in TierPointTransactionInLastOneYr)
            {
                if (txn?.CreditDetails != null)
                {
                    foreach (CreditDetails crd in txn?.CreditDetails)
                    {
                        if (crd?.PointType == TierPointsType)
                        {
                            TierPoints += Convert.ToDouble(crd?.CreditValue);
                        }
                    }
                }
            }

            return Convert.ToString(TierPoints);
        }
    }
}
