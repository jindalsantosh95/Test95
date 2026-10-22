namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{
    public class VacEmailCheckResponse
    {
        public string CustomerId { get; set; }
        public string FcNumber { get; set; }
        public bool isBasic { get; set; }
        public string Tier { get; set; }
    }
}
