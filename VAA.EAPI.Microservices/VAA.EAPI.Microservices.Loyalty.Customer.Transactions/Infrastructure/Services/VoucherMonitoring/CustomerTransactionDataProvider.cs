using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring
{
    public class CustomerTransactionDataProvider : ICustomerTransactionDataProvider
    {
        private readonly AwardsQueryHandler _awardsQueryHandler;
        private readonly TransactionDocumentQueryHandler _transactionDocumentQueryHandler;

        public CustomerTransactionDataProvider(AwardsQueryHandler awardsQueryHandler, TransactionDocumentQueryHandler transactionDocumentQueryHandler)
        {
            _awardsQueryHandler = awardsQueryHandler;
            _transactionDocumentQueryHandler = transactionDocumentQueryHandler;
        }

        public async Task<IEnumerable<Award>> GetAwardsAsync(string customerId, CancellationToken cancellationToken)
        {
            var response = await _awardsQueryHandler.Handle(
                new AwardsQuery
                {
                    CustomerId = customerId,
                    DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType)
                },
                cancellationToken);

            return response.IsSuccess && response.Value?.Errors is null ? response.Value.Value?.Awards : null;
        }

        public async Task<IEnumerable<TransactionDocument>> GetTransactionsAsync(string customerId, CancellationToken cancellationToken)
        {
            var response = await _transactionDocumentQueryHandler.Handle(
                new TransactionQuery
                {
                    CustomerId = customerId,
                    DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType)
                },
                cancellationToken);

            return response.IsSuccess && response.Value?.Errors is null ? response.Value.Value?.TransactionDocuments : null;
        }
    }
}
