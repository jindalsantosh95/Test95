namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses
{
    public class AuditBase
    {
        public string CustomerId { get; set; }
        internal string DocType { get; set; }

        public AuditBase()
        {
            // Required for Serialization
        }
    }
}
