using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface
{
    public interface ITransactionDBServices
    {
        Task<bool> AddTransactionAuditInfo(string membershipNumber, string transactionID, string status, string transactionType, string activityDate, string transactionProcessedDate, string transactionSource, string serviceRequest, string siebelRequest, string siebelResponse, string createdBy, string createdByEmailId, string createdByUserName);
        Task<bool> UpdateVRedResponseAuditInfo(string transactionID, string vRedTransactionResponse);
        Task<bool> UpdateVRedRequestAuditInfo(string transactionID, string vRedTransactionRequest);
    }
}
