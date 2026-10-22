using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember
{
    public class FlyingClubMemberCommandQueryResponse : ICommandStatusQueryResponse
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public FlyingClubMember FlyingClubMemberDocument { get; set; }
        public Status CommandStatus { get; set; }
    }
}

