using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class OrderByReference
    {
        public IEnumerable<ItemsForDeletion> ItemsForDeletion { get; set; }
        public ItemsForAddition ItemsForAddition { get; set; }
        public IEnumerable<string> CustomerReference { get; set; }
    }
}
