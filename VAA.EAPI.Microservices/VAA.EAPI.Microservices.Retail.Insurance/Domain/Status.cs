namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class Status
    {
        public string Code { get; set; }
        public string CreationDate { get; set; }
        public string ProcessId { get; set; }
        public string Version { get; set; }
        public string? ErrorText { get; set; }
        public string? Message { get; set; }
        public string? Reason { get; set; }
        public string Source { get; set; }
    }
}