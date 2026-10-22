namespace VAA.EAPI.Microservices.Inflight.Connectivity.Domain
{
    public sealed class StarlinkClientResponse
    {
        public IEnumerable<string> Errors { get; set; }
        public IEnumerable<string> Warnings { get; set; }
        public IEnumerable<string> Information { get; set; }
        public bool IsValid { get; set; }
    }
}