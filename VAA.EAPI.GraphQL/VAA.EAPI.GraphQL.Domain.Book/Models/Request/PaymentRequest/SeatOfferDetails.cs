namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
{
    public class SeatOfferDetails
    {
        public string DeltaSeatOfferId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string BrandId { get; set; }
        public string BrandDescription { get; set; }
        public string DeltaSeatOfferItemNumber { get; set; }
    }
}