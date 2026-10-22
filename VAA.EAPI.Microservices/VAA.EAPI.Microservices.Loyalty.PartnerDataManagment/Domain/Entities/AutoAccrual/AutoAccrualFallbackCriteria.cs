namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualFallbackCriteria
    {
        public string ProcessingStatus { get; init; } = string.Empty;
        public string NoShowIndicator { get; init; } = string.Empty;
        public string SentToPartnerIndicator { get; init; } = string.Empty;
        public DateTime DepartureDate { get; init; }
        public string FfpCarrierCodes { get; init; } = string.Empty;
    }
}
