namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class ErrorType
    {
        public string AdditionalInfo { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonText { get; set; }
    }
}
