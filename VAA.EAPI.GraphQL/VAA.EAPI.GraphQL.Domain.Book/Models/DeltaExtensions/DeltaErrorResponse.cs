namespace VAA.EAPI.GraphQL.Domain.Book.Models.DeltaExtensions
{
    public class DeltaErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public IEnumerable<Info> MoreInfo { get; set; }
    }
}
