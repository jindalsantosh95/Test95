using System.Collections.Generic;
using ErrorDetails = VAA.EAPI.Domain.Entities.Common.ErrorDetails;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
{
    public class OrderTicketRS
    {
        public OrderTicketDocument OrderTicketDocument { get; set; }

        public IEnumerable<ErrorDetails> Errors { get; set; }

    }
}
