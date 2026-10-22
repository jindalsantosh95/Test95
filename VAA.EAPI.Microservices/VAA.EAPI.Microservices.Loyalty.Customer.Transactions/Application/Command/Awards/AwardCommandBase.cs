using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards
{
    public class AwardCommandBase : TransactionBaseCommand
    {
        public Award Award { get; set; }
        public string ProductId { get; set; }

        public AwardCommandBase()
        {
            Subject = ConfigReader.Get(EnvironmentSettings.AwardEventSubject);
            DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType);
        }


        public static async Task<IEnumerable<Award>> GetListOfAwards(IMediator mediator, string customerId, CancellationToken cancellationToken)
        {
            return (await mediator.Send(new AwardsQuery()
            {
                CustomerId = customerId,
                DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType)
            }, cancellationToken))
                    .Value.Value.Awards;
        }
    }
}
