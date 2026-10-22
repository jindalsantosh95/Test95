namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class OrderErrorResponse
    {
        public bool IsFailure { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
    }

}
