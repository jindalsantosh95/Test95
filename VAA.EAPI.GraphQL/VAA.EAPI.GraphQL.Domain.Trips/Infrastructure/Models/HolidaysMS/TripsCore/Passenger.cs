namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class Passenger
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string NameNumber { get; set; }
        public string PassengerId { get; set; }
        public string AssociatedInfantId { get; set; }
        public LoyaltyMembership LoyaltyMembership { get; set; }
        public PassengerType PassengerType { get; set; }
        public string Title { get; set; }
        public string GivenNames { get; set; }
        public string LastName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public Phone Phone { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public string FirstName { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
    }
}
