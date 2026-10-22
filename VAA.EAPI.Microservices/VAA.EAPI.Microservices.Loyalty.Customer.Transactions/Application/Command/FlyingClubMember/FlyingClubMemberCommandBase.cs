using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember
{
    public class FlyingClubMemberCommandBase : TransactionBaseCommand
    {
        public Domain.FlyingClubMember.FlyingClubMember FlyingClubMemberDocument { get; set; }
        public string HeadTierCode { get; set; }
        public FlyingClubMemberCommandBase() : base()
        {
            Subject = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberEventSubject);
            DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType);
        }

        public static async Task<IEnumerable<Domain.FlyingClubMember.FlyingClubMember>> GetListOfFlyingClubMembers(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new FlyingClubMemberQuery() { CustomerId = customerId, DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType) }, cancellationToken))
                                 .Value.Value.FlyingClubMemberDocuments;
        }
    }
}
