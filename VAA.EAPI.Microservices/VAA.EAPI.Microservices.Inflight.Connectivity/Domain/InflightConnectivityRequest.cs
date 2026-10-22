namespace VAA.EAPI.Microservices.Inflight.Connectivity.Domain
{
    public sealed class InflightConnectivityRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SandboxId { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
