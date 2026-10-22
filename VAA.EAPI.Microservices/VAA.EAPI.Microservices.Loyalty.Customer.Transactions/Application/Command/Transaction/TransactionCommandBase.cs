using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction
{
    public class TransactionCommandBase : TransactionBaseCommand
    {
        public TransactionDocument TransactionDocument { get; set; }
        public string ProductName { get; set; }
        public string PartnerName { get; set; }

        public TransactionCommandBase() : base()
        {
            Subject = ConfigReader.Get(Constants.EnvironmentSettings.TransactionEventSubject);
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDocType);
        }

        public static async Task<IEnumerable<TransactionDocument>> GetListOfTransactions(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new TransactionQuery() { CustomerId = customerId, DocType = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDocType) }, cancellationToken))
                                 .Value.Value.TransactionDocuments;
        }
    }
}

