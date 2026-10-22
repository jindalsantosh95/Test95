namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PassengerBasket
{
    public class People
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string dateOfBirth { get; set; }
        public bool profilePassenger { get; set; }
        public string gender { get; set; }
        public string validation { get; set; }
        public string type { get; set; }
        public MembershipDetails membershipDetails { get; set; }
    }
}
