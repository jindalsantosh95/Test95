using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.GroupDoc
{
    public class GroupDocCommandBase : TransactionBaseCommand
    {
        public GroupDocument GroupDoc { get; set; }

        public GroupDocCommandBase() : base()
        {
            Subject = ConfigReader.Get(EnvironmentSettings.GroupDocTypeEventSubject);
            DocType = ConfigReader.Get(EnvironmentSettings.GroupDocType);
        }

        public static async Task<IEnumerable<GroupDocument>> GetGroupDocuments(IMediator mediator,
            string groupId,
            CancellationToken cancellationToken)
        {
            return (await mediator.Send(new GroupDocQuery(groupId) { DocType = ConfigReader.Get(EnvironmentSettings.GroupDocType) }, cancellationToken))
                .Value.Value.GroupDocuments;
        }
    }
}
