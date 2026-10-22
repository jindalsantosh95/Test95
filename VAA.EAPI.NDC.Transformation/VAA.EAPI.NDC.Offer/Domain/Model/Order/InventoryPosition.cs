using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class InventoryPosition : InventoryLocation
    {
        public int? Quantity { get; set; }
        public string Condition { get; set; }
        public DateTime? AgeMin { get; set; }
        public DateTime? AgeMax { get; set; }

    }
}
