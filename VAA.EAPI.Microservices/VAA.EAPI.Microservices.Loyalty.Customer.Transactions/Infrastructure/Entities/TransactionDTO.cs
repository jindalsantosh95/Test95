using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class TransactionDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
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
        public FlightdetailsDTO FlightDetails { get; set; }
        public FinancialcreditdetailsDTO FinancialCreditDetails { get; set; }
        public List<CreditdetailDTO> CreditDetails { get; set; }
        public List<TransactionNotesDTO> TransactionNotes { get; set; }

        public TransactionDTO()
        {

        }

        public static explicit operator TransactionDTO(TransactionDocument transactionDocument)
        {
            return new()
            {
                SchemaVersion = Constants.Application.TransactionDocSchemaVersion,
                Brand = Constants.Application.TransactionBrand,
                DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType),
                PartitionKey = transactionDocument?.CustomerId,
                CustomerId = transactionDocument?.CustomerId,
                SourceIdentifier = transactionDocument?.SourceIdentifier,
                SourceSystem = transactionDocument?.SourceSystem,
                CreatedOn = string.IsNullOrEmpty(transactionDocument?.TransactionId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : transactionDocument?.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = transactionDocument?.CreatedBy,
                LastUpdatedOn = string.IsNullOrEmpty(transactionDocument?.TransactionId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : transactionDocument?.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = transactionDocument?.LastUpdatedBy,
                CancelledTransactionId = transactionDocument?.CancelledTransactionId,
                CancellationReasonCode = transactionDocument?.CancellationReasonCode,
                Comments = transactionDocument?.Comments,
                ClassCode = transactionDocument?.ClassCode,
                ActivityNumber = transactionDocument?.ActivityNumber,
                TransactionIntegrationId = transactionDocument?.TransactionIntegrationId,
                CurrentLoanBalance = transactionDocument?.CurrentLoanBalance,
                LoanRepaymentDate = transactionDocument?.LoanRepaymentDate,
                MemberNumber = transactionDocument?.MemberNumber,
                TransactionValue = !string.IsNullOrEmpty(transactionDocument?.TransactionValue) ? ((int)Convert.ToDouble(transactionDocument?.TransactionValue)).ToString() : string.Empty,
                ActivityMultiplier = transactionDocument?.ActivityMultiplier,
                OwnerId = transactionDocument?.OwnerId,
                PartnerId = transactionDocument?.PartnerId,
                ParentTransactionId = transactionDocument?.ParentTransactionId,
                ProcessingComment = transactionDocument?.ProcessingComment,
                Processinglog = transactionDocument?.Processinglog,
                TransactionDate = DateTime.Parse(transactionDocument?.TransactionDate).ToString(Constants.Application.DBTimeFormat),
                ProductId = transactionDocument?.ProductId,
                TransactionId = string.IsNullOrEmpty(transactionDocument?.TransactionId) ? Utility.GetId() : transactionDocument?.TransactionId,
                StatusCode = transactionDocument?.StatusCode,
                SubStatus = transactionDocument?.SubStatus,
                TransactionSubType = transactionDocument?.TransactionSubType,
                TransferMemberNumber = transactionDocument?.TransferMemberNumber,
                TransferTransactionId = transactionDocument?.TransferTransactionId,
                TransactionChannel = transactionDocument?.TransactionChannel,
                ActivityDate = DateTime.Parse(transactionDocument?.ActivityDate).ToString(Constants.Application.DBTimeFormat),
                TransactionType = transactionDocument?.TransactionType,
                AwardId = transactionDocument?.AwardId,
                AwardQuantity = transactionDocument?.AwardQuantity,
                VoucherTypeId = transactionDocument?.VoucherTypeId,
                CancelledBy = transactionDocument?.CancelledBy,
                LoanRequiredFlag = transactionDocument?.LoanRequiredFlag,
                StorePartnerNumber = transactionDocument?.StorePartnerNumber,
                StatementText = transactionDocument?.StatementText,
                ReasonCode = transactionDocument?.ReasonCode,
                TierPointsToCredit = !string.IsNullOrEmpty(transactionDocument?.TierPointsToCredit) ? ((int)Convert.ToDouble(transactionDocument?.TierPointsToCredit)).ToString() : string.Empty,
                ManuallyUpdatedBy = transactionDocument?.ManuallyUpdatedBy,
                PDMDocNum = transactionDocument?.PDMDocNum,
                PDMPartnerFileId = transactionDocument?.PDMPartnerFileId,
                PDMRecordId = transactionDocument?.PDMRecordId,
                VGLCExportFlag = transactionDocument?.VGLCExportFlag,
                Id = transactionDocument?.Id,
                PDMFileType = transactionDocument?.PDMFileType,
                NoOfTickets = transactionDocument?.NoOfTickets,
                PointOverrideFlag = transactionDocument?.PointOverrideFlag,
                FlightDetails = (FlightdetailsDTO)transactionDocument?.FlightDetails,
                FinancialCreditDetails = (FinancialcreditdetailsDTO)transactionDocument?.FinancialCreditDetails,
                CreditDetails = transactionDocument?.CreditDetails?.Select(x => (CreditdetailDTO)x).ToList(),
                TransactionNotes = transactionDocument?.TransactionNotes?.Select(x => (TransactionNotesDTO)x).ToList()
            };
        }
    }
}

