namespace VAA.EAPI.Microservices.Inflight.Connectivity.Domain
{
    public sealed class StarlinkClientRequest
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SandboxId { get; set; } = string.Empty;
        public DateTimeOffset Expiry { get; set; }
    }
}