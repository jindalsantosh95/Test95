namespace VAA.EAPI.Microservices.Retail.Lopa.Domain.Lopa
{

    public class Row
    {
        public int? RowNumber { get; set; }
        public bool OverWing { get; set; }
        public List<Column> Columns { get; set; }
        public bool ExitLeft { get; set; }
        public bool ExitRight { get; set; }
    }

}