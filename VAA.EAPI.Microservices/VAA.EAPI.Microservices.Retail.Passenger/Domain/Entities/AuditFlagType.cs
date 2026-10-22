namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class AuditFlagType
    {
        public string DeleteDTI { get; set; }
        public string DeleteDTIForAll { get; set; }
        public string DeleteTKI { get; set; }
        public string QFail { get; set; }
        public string VerifyDTI { get; set; }
        public string VerifyTKI { get; set; }
    }
}
