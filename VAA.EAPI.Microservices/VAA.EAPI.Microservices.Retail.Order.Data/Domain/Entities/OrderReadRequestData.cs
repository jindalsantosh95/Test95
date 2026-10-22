using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class OrderReadRequestData : IRequest<Unit>
    {
        public string Pnr { get; set; }
        public string IssueDate { get; set; }
        public List<TicketDetails> Tickets { get; set; }
    }
}
