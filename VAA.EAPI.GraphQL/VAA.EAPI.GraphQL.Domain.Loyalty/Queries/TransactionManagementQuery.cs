using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TransactionManagementQuery
    {
        private readonly ICustomerTransactionQueryProvider _customerTransactionQueryProvider;
        private readonly IProductsQueryProvider _productsQueryProvider;
        private readonly ICustomerProvider _customerProvider;

        public TransactionManagementQuery(ICustomerTransactionQueryProvider customerTransactionQueryProvider, IProductsQueryProvider productsQueryProvider, ICustomerProvider customerProvider)
        {
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _productsQueryProvider = productsQueryProvider ?? throw new ArgumentNullException(nameof(productsQueryProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        [Authorize(Roles = ["Loyalty.RelatedTransaction.Read"])]
        [GraphQLName("loyaltyRelatedTransactions")]
        [GraphQLDescription("Retreive a list of related transactions")]
        public async Task<TransactionResponse<Transaction>> RelatedTransactions(RelatedTransactionRequest relatedTransactionRequest)
        {
            if (relatedTransactionRequest == null) throw new ArgumentNullException(nameof(relatedTransactionRequest));

            // If customer ID/member number is null in request, then fetch it from customer MS
            if (string.IsNullOrEmpty(relatedTransactionRequest.CustomerId) || string.IsNullOrEmpty(relatedTransactionRequest.MemberNumber))
            {
                CustomerCustMS customerResponse = (await _customerProvider.GetCustomer(relatedTransactionRequest.CustomerId, relatedTransactionRequest.MemberNumber)).Customer;

                relatedTransactionRequest.CustomerId = customerResponse.CustomerId;
                relatedTransactionRequest.MemberNumber = customerResponse.LoyaltyNumber;

            }

            IEnumerable<ProductDetails> productDetails = await _productsQueryProvider.GetProductDetails();

            var excludedProducts = productDetails.Where(p => Constants.Application.RelatedTxnExcludedProducts.Any(x => p.ProductName == x));

            var relatedTxnResponse = await _customerTransactionQueryProvider.RelatedTransactionsQuery(relatedTransactionRequest.CustomerId, relatedTransactionRequest.TransactionId);

            relatedTxnResponse.transactionDocuments = relatedTxnResponse.transactionDocuments.Where(t => !excludedProducts.Any(x => x.RowId == t.ProductId));

            return relatedTxnResponse;
        }
    }
}
