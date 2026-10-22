namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday;

public class PartyComposition
{
    public string? Adults { get; set; }
    public IEnumerable<string> ChildAges { get; set; } = Enumerable.Empty<string>();
    public string? Infants { get; set; }
}