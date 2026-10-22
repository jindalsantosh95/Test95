namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class LoyRetroTransaction
    {
        public string CodeshareFlag { get; set; }
        public string VsComments { get; set; }
        public string ProductName { get; set; }
        public RetroTxnCustomerDetails CustomerDetails { get; set; }
        public BookingDetails BookingDetails { get; set; }
    }
}
