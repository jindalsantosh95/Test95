using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership
{
    public class PartnerMembershipCommandBase : TransactionBaseCommand
    {
        public Domain.PartnerMembership.PartnerMembership PartnerMembershipDocument { get; set; }

        public PartnerMembershipCommandBase() : base()
        {
            Subject = ConfigReader.Get(Constants.EnvironmentSettings.PartnerMembershipEventSubject);
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.PartnerMembershipDocType);
        }

        public static async Task<IEnumerable<Domain.PartnerMembership.PartnerMembership>> GetPartnerMembershipDoc(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new PartnerMembershipQuery() { CustomerId = customerId, DocType = ConfigReader.Get(Constants.EnvironmentSettings.PartnerMembershipDocType) }, cancellationToken))
                                 .Value.Value.PartnerMembershipDocuments;
        }
    }
}
