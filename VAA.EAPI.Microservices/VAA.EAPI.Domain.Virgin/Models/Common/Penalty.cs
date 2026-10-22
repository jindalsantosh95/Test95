namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class Penalty
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public MaxPrice MaxPrice { get; set; }
        public MinPrice MinPrice { get; set; }
        public string Text { get; set; }
    }
}
