namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class Brand
    {
        public ICollection<Itinerary>? Itineraries { get; set; }
        public List<string>? Errors { get; set; }
    }
}
