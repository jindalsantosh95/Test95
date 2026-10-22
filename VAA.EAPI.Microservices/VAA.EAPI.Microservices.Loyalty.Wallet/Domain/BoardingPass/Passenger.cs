namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass
{
    public class Passenger
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerFirstNIN { get; set; }
        public string PassengerLastNIN { get; set; }
        public bool CheckedIn { get; set; }
    }
}
