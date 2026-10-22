using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class OrderByReference
    {
        public List<ItemsForDeletion> ItemsForDeletion { get; set; }
        public ItemsForAddition ItemsForAddition { get; set; }
        public List<string> CustomerReference { get; set; }
    }
}
