using System;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class Link
    {
        public string AdaptiveLinkId { get; set; }
        public string AdaptiveLinkExternalId { get; set; }
        public string Url { get; set; }
        public string IosUrl { get; set; }
        public string AndroidUrl { get; set; }
        public bool IsPersonalized { get; set; }
        public int IosTemplateId { get; set; }
        public int AndroidTemplateId { get; set; }
        public int ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TtlInDays { get; set; }
        public int FlightId { get; set; }
        public string FlightExternalId { get; set; }
        public int Status { get; set; }
        public bool IsExpired { get; set; }
        public Payload Payload { get; set; }
        public bool DoesAllowUrlParameters { get; set; }
        public bool IsAvailablePassesUnlimited { get; set; }
    }
}
