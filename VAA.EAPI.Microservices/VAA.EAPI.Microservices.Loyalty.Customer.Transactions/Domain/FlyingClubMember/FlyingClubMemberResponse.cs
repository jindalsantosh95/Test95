using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember
{
    public class FlyingClubMemberResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<FlyingClubMember> FlyingClubMemberDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public FlyingClubMemberResponse()
        {
            // Required for Serialisation
        }

        public FlyingClubMemberResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;

            // Convert List of 'object' to List of FlyingClubMemberDTO 
            IEnumerable<FlyingClubMemberDTO> dtoList = Json.Deserialize<IEnumerable<FlyingClubMemberDTO>>(Json.Serialize(documents));

            // Convert from DTO to Domain
            FlyingClubMemberDocuments = dtoList.Select(dto => new FlyingClubMember(dto));

        }
    }
}
