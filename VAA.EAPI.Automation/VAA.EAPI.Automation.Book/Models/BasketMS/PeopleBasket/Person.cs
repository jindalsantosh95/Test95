namespace VAA.EAPI.Automation.Book.Models.BasketMS.PeopleBasket
{
    public class Person
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public MembershipDetails MembershipDetails { get; set; }
    }
}
