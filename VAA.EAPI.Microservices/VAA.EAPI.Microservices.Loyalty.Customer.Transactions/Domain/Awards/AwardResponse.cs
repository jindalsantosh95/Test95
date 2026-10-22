using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards
{
    public class AwardResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<Award> Awards { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public AwardResponse()
        {
            // Required for Serialisation
        }

        public AwardResponse(string customerId, IEnumerable<object> transactions)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of ContactNoteDTO (Cant think of a more efficient way)
            IEnumerable<AwardDTO> dtoList = Json.Deserialize<IEnumerable<AwardDTO>>(Json.Serialize(transactions));

            // Convert from DTO to Domain
            Awards = dtoList.Select(dto => new Award(dto));
        }
    }
}
