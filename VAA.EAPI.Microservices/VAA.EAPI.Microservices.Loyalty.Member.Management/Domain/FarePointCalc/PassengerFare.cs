
namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc
{
    public class PassengerFare
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastName { get; set; }
        public string LastNameNumber { get; set; }
        public int FarePoints { get; set; }
    }
}
