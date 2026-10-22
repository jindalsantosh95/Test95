namespace VAA.EAPI.GraphQL.Domain.VHols.Models
{
    public class Error
    {
        public DateTime timestamp { get; set; }
        public int status { get; set; }
        public string error { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string path { get; set; }
    }
}
