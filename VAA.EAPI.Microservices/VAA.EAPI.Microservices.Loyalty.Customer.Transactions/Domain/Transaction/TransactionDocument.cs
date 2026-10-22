using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class TransactionDocument : CustomerTransaction
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CancelledTransactionId { get; set; }
        public string CancellationReasonCode { get; set; }
        public string Comments { get; set; }
        public string ClassCode { get; set; }
        public string ActivityNumber { get; set; }
        public string TransactionIntegrationId { get; set; }
        public string CurrentLoanBalance { get; set; }
        public string LoanRepaymentDate { get; set; }
        public string MemberNumber { get; set; }
        public string TransactionValue { get; set; }
        public string ActivityMultiplier { get; set; }
        public string OwnerId { get; set; }
        public string PartnerId { get; set; }
        public string ParentTransactionId { get; set; }
        public string ProcessingComment { get; set; }
        public string Processinglog { get; set; }
        public string TransactionDate { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string StatusCode { get; set; }
        public string SubStatus { get; set; }
        public string TransactionSubType { get; set; }
        public string TransferMemberNumber { get; set; }
        public string TransferTransactionId { get; set; }
        public string TransactionChannel { get; set; }
        public string ActivityDate { get; set; }
        public string TransactionType { get; set; }
        public string AwardId { get; set; }
        public string AwardName { get; set; }
        public string AwardQuantity { get; set; }
        public string VoucherTypeId { get; set; }
        public string CancelledBy { get; set; }
        public string LoanRequiredFlag { get; set; }
        public string StorePartnerNumber { get; set; }
        public string StatementText { get; set; }
        public string ReasonCode { get; set; }
        public string TierPointsToCredit { get; set; }
        public string ManuallyUpdatedBy { get; set; }
        public string PDMDocNum { get; set; }
        public string PDMPartnerFileId { get; set; }
        public string PDMRecordId { get; set; }
        public string VGLCExportFlag { get; set; }
        public string PDMFileType { get; set; }
        public string NoOfTickets { get; set; }
        public string PointOverrideFlag { get; set; }
        public Flightdetails FlightDetails { get; set; }
        public Financialcreditdetails FinancialCreditDetails { get; set; }
        public List<Creditdetail> CreditDetails { get; set; }
        public List<TransactionNotes> TransactionNotes { get; set; }
        public TransactionDocument()
        {

        }
        public TransactionDocument(TransactionDTO transactionDocument)
        {
            CustomerId = transactionDocument?.CustomerId;
            SourceIdentifier = transactionDocument?.SourceIdentifier;
            SourceSystem = transactionDocument?.SourceSystem;
            CreatedOn = Common.Helpers.DateTimeHelper.ParseDateFormat(transactionDocument?.CreatedOn) ?? DateTime.Parse(transactionDocument?.CreatedOn);
            CreatedBy = transactionDocument?.CreatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(transactionDocument.LastUpdatedOn) ? DateTime.Parse(transactionDocument.LastUpdatedOn) : DateTime.Now.ToUniversalTime();
            LastUpdatedBy = transactionDocument?.LastUpdatedBy;
            CancelledTransactionId = transactionDocument?.CancelledTransactionId;
            CancellationReasonCode = transactionDocument?.CancellationReasonCode;
            Comments = transactionDocument?.Comments;
            ClassCode = transactionDocument?.ClassCode;
            ActivityNumber = transactionDocument?.ActivityNumber;
            TransactionIntegrationId = transactionDocument?.TransactionIntegrationId;
            CurrentLoanBalance = transactionDocument?.CurrentLoanBalance;
            LoanRepaymentDate = transactionDocument?.LoanRepaymentDate;
            MemberNumber = transactionDocument?.MemberNumber;
            TransactionValue = transactionDocument?.TransactionValue;
            ActivityMultiplier = transactionDocument?.ActivityMultiplier;
            OwnerId = transactionDocument?.OwnerId;
            PartnerId = transactionDocument?.PartnerId;
            ParentTransactionId = transactionDocument?.ParentTransactionId;
            ProcessingComment = transactionDocument?.ProcessingComment;
            Processinglog = transactionDocument?.Processinglog;
            TransactionDate = transactionDocument?.TransactionDate;
            ProductId = transactionDocument?.ProductId;
            TransactionId = transactionDocument?.TransactionId;
            StatusCode = transactionDocument?.StatusCode;
            SubStatus = transactionDocument?.SubStatus;
            TransactionSubType = transactionDocument?.TransactionSubType;
            TransferMemberNumber = transactionDocument?.TransferMemberNumber;
            TransferTransactionId = transactionDocument?.TransferTransactionId;
            TransactionChannel = transactionDocument?.TransactionChannel;
            ActivityDate = transactionDocument?.ActivityDate;
            TransactionType = transactionDocument?.TransactionType;
            AwardId = transactionDocument?.AwardId;
            AwardQuantity = transactionDocument?.AwardQuantity;
            VoucherTypeId = transactionDocument?.VoucherTypeId;
            CancelledBy = transactionDocument?.CancelledBy;
            LoanRequiredFlag = transactionDocument?.LoanRequiredFlag;
            StorePartnerNumber = transactionDocument?.StorePartnerNumber;
            StatementText = transactionDocument?.StatementText;
            ReasonCode = transactionDocument?.ReasonCode;
            TierPointsToCredit = transactionDocument?.TierPointsToCredit;
            ManuallyUpdatedBy = transactionDocument?.ManuallyUpdatedBy;
            PDMDocNum = transactionDocument?.PDMDocNum;
            PDMPartnerFileId = transactionDocument?.PDMPartnerFileId;
            PDMRecordId = transactionDocument?.PDMRecordId;
            VGLCExportFlag = transactionDocument?.VGLCExportFlag;
            Id = transactionDocument?.Id;
            PDMFileType = transactionDocument?.PDMFileType;
            NoOfTickets = transactionDocument?.NoOfTickets;
            PointOverrideFlag = transactionDocument?.PointOverrideFlag;
            if (transactionDocument.FlightDetails != null)
            {
                FlightDetails = new Flightdetails(transactionDocument.FlightDetails);
            }
            if (transactionDocument.FinancialCreditDetails != null)
            {
                FinancialCreditDetails = new Financialcreditdetails(transactionDocument.FinancialCreditDetails);
            }
            CreditDetails = new List<Creditdetail>();
            if (transactionDocument.CreditDetails != null)
            {
                foreach (CreditdetailDTO details in transactionDocument.CreditDetails)
                {
                    Creditdetail creditdetail = new Creditdetail(details);
                    CreditDetails.Add(creditdetail);
                }
            }
            TransactionNotes = new List<TransactionNotes>();
            if (transactionDocument.TransactionNotes != null)
            {
                foreach (TransactionNotesDTO notesDTO in transactionDocument.TransactionNotes)
                {
                    TransactionNotes transactionNotes = new TransactionNotes(notesDTO);
                    TransactionNotes.Add(transactionNotes);
                }
            }
        }

        public static explicit operator TransactionDocument((RedeemVoucherRequest rdmVchrDetails, string partnerId, string productId, string awardId, string txnType) txnRequest)
        {
            if (txnRequest.rdmVchrDetails == null) throw new ArgumentNullException(nameof(txnRequest.rdmVchrDetails));
            if (txnRequest.partnerId == null) throw new ArgumentNullException(nameof(txnRequest.partnerId));
            if (txnRequest.productId == null) throw new ArgumentNullException(nameof(txnRequest.productId));

            return new TransactionDocument
            {
                TransactionId = Utility.GetId(),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = txnRequest.rdmVchrDetails.InitiatedBy,
                LastUpdatedOn = DateTime.UtcNow,
                LastUpdatedBy = txnRequest.rdmVchrDetails.InitiatedBy,
                CustomerId = txnRequest.rdmVchrDetails.CustomerId,
                MemberNumber = txnRequest.rdmVchrDetails.MemberNumber,
                TransactionValue = txnRequest.txnType.Is(Constants.Application.AccrualTransactionType) ? txnRequest.rdmVchrDetails.VoucherDetails.PointsToAward.ToString() : "0",
                PartnerId = txnRequest.partnerId,
                TransactionDate = DateTime.UtcNow.ToString(),
                ProductId = txnRequest.productId,
                StatusCode = Constants.Application.StatusCode,
                TransactionSubType = Constants.Application.TxnSubType,
                TransactionChannel = txnRequest.rdmVchrDetails.TransactionChannel,
                ActivityDate = txnRequest.txnType.Is(Constants.Application.AccrualTransactionType) ? DateTime.UtcNow.Date.ToString() : txnRequest.rdmVchrDetails.FlightDetails?.ActivityDate.ToString(),
                TransactionType = txnRequest.txnType,
                AwardId = string.IsNullOrEmpty(txnRequest.awardId) ? string.Empty : txnRequest.awardId,
                FlightDetails = new Flightdetails()
                {
                    BookingClass = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.BookingClass) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.BookingClass,
                    OriginAirportcode = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.OriginAirport) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.OriginAirport,
                    DestinationAirport = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.DestinationAirport) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.DestinationAirport,
                    ExternalReservationNumber = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.BookingReference) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.BookingReference,
                    Ticketnumber = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.TicketNumber) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.TicketNumber,
                    NumberOfSegments = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.NoOfSegments?.ToString()) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.NoOfSegments.ToString(),
                    Quantity = string.IsNullOrEmpty(txnRequest.rdmVchrDetails.FlightDetails?.NoOfPassenger?.ToString()) ? string.Empty : txnRequest.rdmVchrDetails.FlightDetails.NoOfPassenger.ToString()
                },
                CreditDetails = txnRequest.txnType.Is(Constants.Application.AccrualTransactionType) ? new List<Creditdetail>()
                    {
                        new Creditdetail()
                        {
                            CreatedBy =  txnRequest.rdmVchrDetails.InitiatedBy,
                            LastUpdatedBy = txnRequest.rdmVchrDetails.InitiatedBy,
                            CreditValue = txnRequest.rdmVchrDetails.VoucherDetails.PointsToAward.ToString(),
                            PointType = Constants.Application.MilesType,
                            PointSubType = Constants.Application.BaseMilesType,
                            QualifyingFlag = Constants.Application.MilesQualifyingFlag
                        }
                    } : null
            };
        }

        public static explicit operator TransactionDocument((RollbackVoucherRdmRequest rollbackRequest, TransactionDocument txnDetail, string awardName) source)
        {
            if (source.rollbackRequest == null) throw new ArgumentNullException(nameof(source.rollbackRequest));
            if (source.txnDetail == null) throw new ArgumentNullException(nameof(source.txnDetail));

            return new TransactionDocument
            {
                CustomerId = source.rollbackRequest.CustomerId,
                MemberNumber = source.rollbackRequest.MemberNumber,
                TransactionId = source.txnDetail.TransactionId,
                TransactionChannel = source.rollbackRequest.RollbackSource,
                CancelledBy = source.rollbackRequest.RollbackInitiatedBy,
                CancellationReasonCode = Constants.Application.RollbackReasonCode,
                LastUpdatedOn = DateTime.UtcNow,
                AwardName = source.awardName
            };
        }
    }
}
