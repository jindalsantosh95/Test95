namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class SkyPartnerRetroRequest
    {
        public LoyRetroTransaction LoyTransaction { get; set; }
        public string TransactionChannel { get; set; }
        public string CreatedBy { get; set; }
        public bool NonVARetroOverrideFlag { get; set; }
    }
}
