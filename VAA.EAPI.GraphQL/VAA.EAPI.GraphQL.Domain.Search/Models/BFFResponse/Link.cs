namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class Link
    {
        public string? Rel { get; set; }

        private string _href;
        public string Href
        {
            get
            {
                return _href;
            }
            set
            {
                _href = value;
            }
        }
    }
}
