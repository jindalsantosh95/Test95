namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class FareDetails
    {
        public string FareBasisCode { get; set; }
        public string FareFamilyType { get; set; }
        public IEnumerable<FareSegments> FareSegments { get; set; }
        public string FareCalcText { get; set; }
    }
}
