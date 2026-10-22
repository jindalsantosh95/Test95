using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class InviteHouseHoldResponse : ICustomerTransactionQueryResponse
    {
        public string CustomerId { get; set; } = string.Empty;

        public IEnumerable<InviteHouseHold> InviteHouseHold { get; set; }

        public InviteHouseHoldResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            if (documents.Any())
            {
                IEnumerable<InviteHouseHoldDTO> dtoInviteHouseHold = Json.Deserialize<IEnumerable<InviteHouseHoldDTO>>(Json.Serialize(documents));
                InviteHouseHold = dtoInviteHouseHold.Select(dto => new InviteHouseHold(dto));
            }
        }
    }
}
