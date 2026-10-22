using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class AuditTransactionInfo
    {
        public string MembershipNumber { get; set; }
        public string TransactionID { get; set; }
        public string TransactionType { get; set; }
        public string ActivityDate { get; set; }
        public string Status { get; set; }
        public string TransactionProcessedDate { get; set; }
        public string TransactionSource { get; set; }
        public string ServiceRequest { get; set; }
        public string SiebelRequest { get; set; }
        public string SiebelResponse { get; set; }
        public string VredRequest { get; set; }
        public string VredResponse { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByEmailId { get; set; }
        public string CreatedByUserName { get; set; }

        public static explicit operator AuditTransactionInfo((UpdateTransactionPnrRequest eventRequest, UpdateTransactionPnrDetails updateDetails, CreditMetaData? earnTransactionDetails, DebitMetaData? burnTransactionDetails) source)
        {
            if (source.eventRequest == null) throw new ArgumentNullException(nameof(source.eventRequest));
            if (source.updateDetails == null) throw new ArgumentNullException(nameof(source.updateDetails));

            return new AuditTransactionInfo
            {
                MembershipNumber = source.eventRequest.MemberNumber,
                TransactionID = source.updateDetails.TransactionId,
                TransactionType = source.updateDetails.TransactionType,
                ActivityDate = source.updateDetails.TransactionType == Constants.Application.AccrualType ? source.earnTransactionDetails?.ActivityDatetime : source.burnTransactionDetails?.ActivityDatetime,
                Status = source.updateDetails.TransactionType == Constants.Application.AccrualType ? source.earnTransactionDetails?.Status : source.burnTransactionDetails?.Status,
                TransactionProcessedDate = DateTime.UtcNow.ToString(Constants.Application.VRedRequestDateFormat),
                TransactionSource = source.updateDetails.TransactionType == Constants.Application.AccrualType ? source.earnTransactionDetails?.TransactionChannelId : source.burnTransactionDetails?.TransactionChannelId,
                ServiceRequest = Json.Serialize(new UpdateTransactionPnrRequest
                {
                    MemberNumber = source.eventRequest.MemberNumber,
                    UpdatedBy = source.eventRequest.UpdatedBy,
                    TransactionDetails = [source.updateDetails]
                }),
                CreatedBy = source.eventRequest.UpdatedBy,
                CreatedByEmailId = source.eventRequest.UpdatedBy,
                CreatedByUserName = source.eventRequest.UpdatedBy
            };
        }
    }
}
