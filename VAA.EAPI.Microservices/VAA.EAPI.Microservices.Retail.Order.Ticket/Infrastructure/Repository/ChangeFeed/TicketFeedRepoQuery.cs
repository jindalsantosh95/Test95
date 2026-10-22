using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed
{
    public class TicketFeedRepoQuery : IRequest<IEnumerable<object>>
    {
        public string Value { set; get; }
        public string DocType { set; get; }
        public string QueryFilter { set; get; }

        public TicketFeedRepoQuery() { }

        public TicketFeedRepoQuery(string value, string docType, string searchFilter)
        {
            Value = value;
            DocType = docType;
            QueryFilter = searchFilter;
        }
    }
}
