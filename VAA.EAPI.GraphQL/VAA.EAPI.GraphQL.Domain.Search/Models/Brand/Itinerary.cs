namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class Itinerary
    {
        public bool IsValid { get; set; }
        public ICollection<Leg>? Legs { get; set; }
        public Amenities? Amenities { get; set; }
    }
}