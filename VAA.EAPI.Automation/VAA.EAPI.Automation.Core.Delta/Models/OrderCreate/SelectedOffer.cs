namespace VAA.EAPI.Automation.Core.Delta.Models.OrderCreate
{
    public class SelectedOffer
    {
        public string offerId { get; set; }
        public List<SelectedOfferItem> selectedOfferItems { get; set; }
        public TotalOfferAmt totalOfferAmt { get; set; }
        public List<string> formOfPaymentId { get; set; }
        public OrderCriteria orderCriteria { get; set; }
    }
}