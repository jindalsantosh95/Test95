namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Channel
{
    public class Channel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool ValidateTitle { get; set; }
        public bool ValidateCustomerAge { get; set; }
        public bool ValidateCountry { get; set; }
        public bool ValidateEmail { get; set; }
    }
}
