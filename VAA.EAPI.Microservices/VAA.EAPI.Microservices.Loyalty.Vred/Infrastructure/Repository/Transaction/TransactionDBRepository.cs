using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;


namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction
{
    public class TransactionDBRepository : ITransactionDBRepository
    {
        private readonly ITransactionDBServices _transactionDBServices;

        public TransactionDBRepository(ITransactionDBServices transactionDBServices)
        {
            _transactionDBServices = transactionDBServices ?? throw new ArgumentNullException(nameof(transactionDBServices));
        }

        public async Task<bool> AddTransactionAuditInfo(string membershipNumber, string transactionID, string status, string transactionType, string activityDate, string transactionProcessedDate, string transactionSource, string serviceRequest, string siebelRequest, string siebelResponse, string createdBy, string createdByEmailId, string createdByUserName)
        {
            return await _transactionDBServices.AddTransactionAuditInfo(membershipNumber, transactionID, status, transactionType, activityDate, transactionProcessedDate, transactionSource, serviceRequest, siebelRequest, siebelResponse, createdBy, createdByEmailId, createdByUserName);
        }

        public async Task<bool> UpdateVRedResponseAuditInfo(string transactionID, string vRedTransactionResponse)
        {
            return await _transactionDBServices.UpdateVRedResponseAuditInfo(transactionID, vRedTransactionResponse);
        }

        public async Task<bool> UpdateVRedRequestAuditInfo(string transactionID, string vRedTransactionRequest)
        {
            return await _transactionDBServices.UpdateVRedRequestAuditInfo(transactionID, vRedTransactionRequest);
        }

    }
}
