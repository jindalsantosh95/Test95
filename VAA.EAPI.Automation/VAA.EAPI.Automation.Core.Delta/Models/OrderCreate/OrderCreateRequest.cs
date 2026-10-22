namespace VAA.EAPI.Automation.Core.Delta.Models.OrderCreate
{
    public class OrderCreateRequest
    {
        public PointOfSale pointOfSale { get; set; }
        public List<SelectedOffer> selectedOffers { get; set; }
        public List<FormsOfPayment> formsOfPayment { get; set; }
        public List<Customer> customers { get; set; }
    }
}