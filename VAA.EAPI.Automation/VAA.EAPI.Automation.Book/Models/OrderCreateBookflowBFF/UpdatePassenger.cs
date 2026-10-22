namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF
{
    public class UpdatePassenger
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public int Age { get; set; }
        public MembershipDetails membershipDetails { get; set; }
    }
}
