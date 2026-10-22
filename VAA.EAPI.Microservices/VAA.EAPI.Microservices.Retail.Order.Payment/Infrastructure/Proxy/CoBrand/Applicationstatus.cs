using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Applicationstatus
    {
        public string ApplicationKey { get; set; }
        public string OfferID { get; set; }
        public string OfferExpiryDate { get; set; }
        public string ApplicationDecision { get; set; }
        [JsonProperty("cipher.couponNumber")]
        public string CiphercouponNumber { get; set; }
        [JsonProperty("cipher.couponExpiryDate")]
        public string CiphercouponExpiryDate { get; set; }
        public string ProductCode { get; set; }
        [JsonProperty("cipher.accountNumber")]
        public string CipheraccountNumber { get; set; }
        [JsonProperty("cipher.temporaryAccountNumber")]
        public string CiphertemporaryAccountNumber { get; set; }
        [JsonProperty("cipher.securityCode")]
        public string CiphersecurityCode { get; set; }
        public string CreditLimit { get; set; }
        public string TemporaryCreditLimit { get; set; }
        [JsonProperty("cipher.temporaryCardExpiry")]
        public string CiphertemporaryCardExpiry { get; set; }
        [JsonProperty("cipher.CardExpiry")]
        public string CipherCardExpiry { get; set; }
        public string MmcCode { get; set; }
        public string MmcDescription { get; set; }
        public string AnnualPurchaseRate { get; set; }
        public string DailyRate { get; set; }
        public string CashAdvanceAPR { get; set; }
        public string CashAdvanceDailyRate { get; set; }
        public string DelinquencyApr { get; set; }
        public string DelinquencyRate { get; set; }
        public string TierCode { get; set; }
        public string CAOPTIN { get; set; }
        public string CreditBureauPullDate { get; set; }
        public string FlowType { get; set; }
        public string IntegrationChannel { get; set; }
        public string FicoScore { get; set; }
        public string BureauCode { get; set; }
        public string CiphercardArtURL { get; set; }
        public Multiprequaloffersstatu[] MultiPrequalOffersStatus { get; set; }
    }
}
