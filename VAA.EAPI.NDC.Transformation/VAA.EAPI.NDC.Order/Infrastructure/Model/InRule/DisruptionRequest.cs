using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class DisruptionRequest
    {
        public Event Event { get; set; }
        public List<OrderReference> Order { get; set; }

        [JsonIgnore]
        public string OrderId => Order?.FirstOrDefault()?.OrderNumber?.OrderId;

        [JsonIgnore]
        public string BookingReference => Order?.FirstOrDefault()?.BookingReference?.RLOC;

        [JsonIgnore]
        public string CorrelationId => Event?.CorrelationId;

    }
}
