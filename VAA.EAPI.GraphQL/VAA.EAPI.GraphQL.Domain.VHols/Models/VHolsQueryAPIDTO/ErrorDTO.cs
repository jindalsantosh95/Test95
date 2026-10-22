namespace VAA.EAPI.GraphQL.Domain.VHols.Models.VHolsQueryAPIDTO
{
    internal class ErrorDTO
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
        public string Error { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
    }
}