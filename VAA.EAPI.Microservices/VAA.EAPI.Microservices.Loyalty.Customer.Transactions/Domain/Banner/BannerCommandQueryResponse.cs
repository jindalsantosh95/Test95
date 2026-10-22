using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner
{
    public class BannerCommandQueryResponse : ICommandStatusQueryResponse
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public BannerDetail Banner { get; set; }
        public Status CommandStatus { get; set; }
    }
}
