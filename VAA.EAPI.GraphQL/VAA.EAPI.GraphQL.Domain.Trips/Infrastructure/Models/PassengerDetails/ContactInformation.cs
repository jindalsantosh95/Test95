namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class ContactInformation
    {
        public Email Email { get; set; }
        public Phone Phone { get; set; }
        public bool Refused { get; set; }
        public string RelationshipToPassenger { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public bool ApplyToAllPassengers { get; set; }
    }
}