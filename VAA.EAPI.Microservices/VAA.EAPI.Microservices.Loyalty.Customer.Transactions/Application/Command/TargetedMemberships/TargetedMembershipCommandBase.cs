using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships
{
    public class TargetedMembershipCommandBase : TransactionBaseCommand
    {
        public TargetedMembership TargetedMembershipDocument { get; set; }

        public TargetedMembershipCommandBase() : base()
        {
            Subject = ConfigReader.Get(EnvironmentSettings.TargetedMembershipEventSubject);
            DocType = ConfigReader.Get(EnvironmentSettings.TargetedMembershipDocType);
        }

        public static async Task<IEnumerable<TargetedMembership>> GetListOfTargetedMemberships(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new TargetedMembershipQuery() { CustomerId = customerId, DocType = ConfigReader.Get(EnvironmentSettings.TargetedMembershipDocType) }, cancellationToken))
                                 .Value.Value.TargetedMembershipDocuments;
        }
    }
}
