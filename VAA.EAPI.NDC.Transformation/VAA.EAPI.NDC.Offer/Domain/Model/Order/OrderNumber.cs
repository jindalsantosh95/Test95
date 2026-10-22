namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderNumber
    {
        public string OrderId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
        public Link Link { get; set; }


    }
}
