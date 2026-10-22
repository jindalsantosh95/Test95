namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{  

    public class SynchronyGenerateTokenResponse
    {
        public string Code { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public string TrackingId { get; set; }
        public string DocumentationUrl { get; set; }
        public string RemediationInfo { get; set; }
        public bool Transient { get; set; }
        public Debuginfo DebugInfo { get; set; }
        public Businesserror[] BusinessErrors { get; set; }
    }

}
