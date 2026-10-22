using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Base
{
    public class EventStatusQueryResponse
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public Status CommandStatus { get; set; }
    }
}
