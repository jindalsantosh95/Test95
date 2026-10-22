namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class FareDetail
    {
        public string FareBasisCode { get; set; }
        public string FareFamilyType { get; set; }
        public IEnumerable<FareSegment> FareSegments { get; set; }
    }
}
