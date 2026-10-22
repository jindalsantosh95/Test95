using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MembershipChange
{
    public class MembershipChangeCommandBase : TransactionBaseCommand
    {
        public Domain.MembershipChange.MembershipChange MembershipChangeDocument { get; set; }

        public MembershipChangeCommandBase() : base()
        {
            Subject = ConfigReader.Get(Constants.EnvironmentSettings.MembershipChangeDocType);
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.MembershipChangeDocType);
        }

        public static async Task<IEnumerable<Domain.MembershipChange.MembershipChange>> GetMembershipChangeDocs(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new MembershipChangeQuery() { CustomerId = customerId, DocType = ConfigReader.Get(Constants.EnvironmentSettings.MembershipChangeDocType) }, cancellationToken))
                                 .Value.Value.MembershipChangeDocuments;
        }
    }
}
