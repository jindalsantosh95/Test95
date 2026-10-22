using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models
{
    public class SubscriptionResponse
    {
        public string SubscriptionState { get; set; }
        public List<SubscriptionKey> SubscriptionKeys { get; set; }
        public string Channel { get; set; }
        public string Environment { get; set; }
        public ErrorDetails Error { get; set; }
        public Status Status { get; set; }
    }
}
