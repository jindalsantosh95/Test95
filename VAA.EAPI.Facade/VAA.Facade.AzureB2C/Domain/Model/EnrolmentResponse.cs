namespace VAA.Facade.AzureB2C.Domain.Model
{
    public class EnrolmentResponse
    {
        public string CustomerId { get; set; }
        public Memberdetails Memberdetails { get; set; }
    }
    public class Memberdetails
    {
        public string Status { get; set; }
    }
}
