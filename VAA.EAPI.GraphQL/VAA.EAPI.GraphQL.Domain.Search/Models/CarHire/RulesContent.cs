namespace VAA.EAPI.GraphQL.Domain.Search.Models.CarHire
{
    public class RulesContent
    {
        public string? LongDescription { get; set; }
        public IEnumerable<CarImage> Images { get; set; } = Enumerable.Empty<CarImage>();
        public string? Name { get; set; }
        public bool IsCloseButtonEnabled { get; set; }
        public string? Description { get; set; }
    }

}
