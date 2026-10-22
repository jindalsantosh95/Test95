using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
{
    public class OrderItemServices
    {
        public IEnumerable<string> RetainServices { get; set; }
        public IEnumerable<string> DeleteServices { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public string OrderItemId { get; set; }
        public string OrderProductType { get; set; }
    }
}
