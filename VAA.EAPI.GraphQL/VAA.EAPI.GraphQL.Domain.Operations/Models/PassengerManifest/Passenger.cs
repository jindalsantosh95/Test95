using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class Passenger
    {
        [GraphQLName("PNRRefNumber")]
        public string PNRRefNumber { get; internal set; }
        [GraphQLName("PNRCreationDate")]
        public DateTime PNRCreationDate { get; internal set; }
        public string PaxNumber { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ChildAge { get; set; }
        public string Gender { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string BookedCabin { get; set; }
        public string BookedClass { get; set; }
        public string BookingStatus { get; set; }
        public Seat Seat { get; set; }
        public LoyaltyDetails LoyaltyDetails { get; set; }
        [UseFiltering]
        [UseSorting]
        public StaffDetails StaffDetails { get; set; }
        public Profile Profile { get; set; } = new();
        public LapInfant LapInfant { get; set; }
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
