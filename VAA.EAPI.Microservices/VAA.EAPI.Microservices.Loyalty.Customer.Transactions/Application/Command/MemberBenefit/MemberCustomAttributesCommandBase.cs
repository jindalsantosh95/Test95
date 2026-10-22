using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit
{
    public class MemberCustomAttributesCommandBase : TransactionBaseCommand
    {
        public MemberCustomAttributes MemberCustomAttributesDocument { get; set; }

        public MemberCustomAttributesCommandBase() : base()
        {
            Subject = ConfigReader.Get(Constants.EnvironmentSettings.MemberCustomAttributesEventSubject);
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.MemberCustomAttributesDocType);
        }

        public static async Task<IEnumerable<MemberCustomAttributes>> GetListOfMemberCustomAttributes(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new MemberCustomAttributesQuery() { CustomerId = customerId, DocType = ConfigReader.Get(Constants.EnvironmentSettings.MemberCustomAttributesDocType) }, cancellationToken))
                                 .Value.Value.MemberCustomAttributesDocuments;
        }
    }
}
