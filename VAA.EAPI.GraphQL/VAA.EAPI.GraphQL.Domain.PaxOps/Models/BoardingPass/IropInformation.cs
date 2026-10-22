namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public class IropInformation
    {
        public IropCard? Card { get; set; }
        public IropPage? Page { get; set; }
    }

    public class IropCard
    {
        public string? Title { get; set; }
        public IEnumerable<string>? Lines { get; set; }
        [HotChocolate.GraphQLIgnore]
        public IEnumerable<string>? MobileLines { get; set; }
    }

    public class IropPage
    {
        public string? Title { get; set; }
        public IEnumerable<IropContentSection>? Sections { get; set; }
        public IEnumerable<string>? NextActionText { get; set; }
    }

    public class IropContentSection
    {
        public string? Heading { get; set; }
        public IEnumerable<string>? Paragraphs { get; set; }
    }
}
