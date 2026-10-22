namespace VAA.EAPI.Infrastructure.Email.Entities
{
    public class CcpResponse
    {
        public object ErrorCode { get; set; }
        public object ErrorMessage { get; set; }
        public string FcNumber { get; set; }
        public bool Success { get; set; }
        public string Tier { get; set; }
        public string Territory { get; set; }
        public string Virgin_Points { get; set; }
        public string Member_Name { get; set; }
    }
}
