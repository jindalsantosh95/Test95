namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat
{
    public class Row
    {
        public int? RowNumber { get; set; }
        public bool OverWing { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public bool ExitLeft { get; set; }
        public bool ExitRight { get; set; }
    }
}