using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction
{
    public class Transaction
    {
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
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
        public DateTime? TransactionDate { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string StatusCode { get; set; }
        public string SubStatus { get; set; }
        public string TransactionSubType { get; set; }
        public string TransferMemberNumber { get; set; }
        public string TransferTransactionId { get; set; }
        public string TransactionChannel { get; set; }
        public DateTime? ActivityDate { get; set; }
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
        public FlightDetails FlightDetails { get; set; }
        public FinancialCreditDetails FinancialCreditDetails { get; set; }
        public IEnumerable<CreditDetails> CreditDetails { get; set; }
        public IEnumerable<TransactionNotes> TransactionNotes { get; set; }
    }
}
