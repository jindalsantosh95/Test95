using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class TransactionResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<TransactionDocument> TransactionDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public TransactionResponse()
        {
            // Required for Serialisation
        }

        public TransactionResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of TransactionDTO
            IEnumerable<TransactionDTO> dtoList = Json.Deserialize<IEnumerable<TransactionDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            TransactionDocuments = dtoList.Select(dto => new TransactionDocument(dto));
        }
    }
}
