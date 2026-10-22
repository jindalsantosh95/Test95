using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class OrderByReference
    {
        public IEnumerable<ItemsForDeletion> ItemsForDeletion { get; set; }
        public ItemsForAddition ItemsForAddition { get; set; }
        public IEnumerable<string> CustomerReference { get; set; }
    }
}
