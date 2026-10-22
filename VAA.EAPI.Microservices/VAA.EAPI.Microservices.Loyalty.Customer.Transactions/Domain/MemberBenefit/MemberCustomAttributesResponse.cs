using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit
{
    public class MemberCustomAttributesResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<MemberCustomAttributes> MemberCustomAttributesDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public MemberCustomAttributesResponse()
        {
            // Required for Serialisation
        }

        public MemberCustomAttributesResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of MemberCustomAttributesDTO (Cant think of a more efficient way)
            IEnumerable<MemberCustomAttributesDTO> dtoList = Json.Deserialize<IEnumerable<MemberCustomAttributesDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            MemberCustomAttributesDocuments = dtoList.Select(dto => new MemberCustomAttributes(dto));
        }
    }
}
