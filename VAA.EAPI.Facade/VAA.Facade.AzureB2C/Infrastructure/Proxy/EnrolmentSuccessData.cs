namespace VAA.Facade.AzureB2C.Infrastructure.Proxy
{

    public class EnrolmentSuccessData
    {
        public string CustomerId { get; set; }
        public Memberdetails Memberdetails { get; set; }
    }
    public class Memberdetails
    {
        public string Status { get; set; }
    }

}
